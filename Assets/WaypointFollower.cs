using UnityEngine;
using System.Collections.Generic;

public class PipelineManager : MonoBehaviour
{
    [Header("İstasyonlar (StationScript)")]
    [Tooltip("Her station (waypoint) üzerinde StationScript bulunmalı. Dizide sırası, ürünlerin gideceği sırayı belirler.")]
    public StationScript[] stations;

    [Header("Ürün Prefab ve Üretim Ayarları")]
    public GameObject productPrefab;
    public int totalProductCount = 5; // Kaç ürün üretilecek?

    [Header("Hareket ve Dönüş Ayarları")]
    public float moveSpeed = 3f;
    public float rotationStepTime = 1f;

    // İstasyonda hangi ürün var? (null = boş)
    // Bu dizinin uzunluğu stations.Length kadar olacak
    private ProductData[] stationOccupants;

    // Sahnedeki tüm ürünler
    private List<ProductData> products = new List<ProductData>();
    private int spawnedCount = 0;

    private void Start()
    {
        if (stations == null || stations.Length == 0)
        {
            Debug.LogError("Station dizisi boş! En az 1 station ekleyin.");
            return;
        }
        if (productPrefab == null)
        {
            Debug.LogError("Product Prefab atanmamış!");
            return;
        }

        // stationOccupants dizisi, stations.Length kadar boyutlu olacak
        stationOccupants = new ProductData[stations.Length];

        // İlk ürün üretmeyi deneyelim
        TrySpawnAtStation0();
    }

    private void Update()
    {
        // Mevcut ürünleri tek tek güncelle
        for (int i = 0; i < products.Count; i++)
        {
            UpdateProduct(products[i]);
        }

        // Her frame 0. istasyon boşaldıysa yeni ürün üretilebilir
        TrySpawnAtStation0();
    }

    /// <summary>
    /// 0. istasyon boşsa ve hâlâ üretilecek ürün varsa yeni bir product oluşturur.
    /// </summary>
    private void TrySpawnAtStation0()
    {

        if (spawnedCount >= totalProductCount) return; // Yeterince ürettik
        if (stationOccupants.Length == 0) return;      // Station yok

        // 0. station dolu mu?
        if (stationOccupants[0] != null) return;       // Doluysa çık

        // Ürün yarat
        GameObject go = Instantiate(productPrefab);
        go.name = "Product_" + spawnedCount;
        var productScript = go.GetComponent<ProductScript>();
        if (productScript != null)
        {
            // stations[0] -> ilk istasyonun adını ver
            productScript.SetStationName(stations[0].gameObject.name);
        }


        // ProductData
        ProductData pd = new ProductData(go);
        pd.currentStationIndex = 0;

        // Konum -> stations[0] objesinin position'ına
        // StationScript de bir Transform içeren objede çalışıyorsa:
        go.transform.position = stations[0].transform.position;

        // Kaydet
        products.Add(pd);
        stationOccupants[0] = pd;

        spawnedCount++;
    }

    /// <summary>
    /// Her kare ürünü güncelle (hareket + rotasyon).
    /// </summary>
    private void UpdateProduct(ProductData prod)
    {
        if (prod.isDestroyed) return; // Zaten silinmişse

        int stIndex = prod.currentStationIndex;

        // Eğer son istasyona vardığı ve finalStationDone = true ise => yok et
        if (stIndex == stations.Length - 1 && prod.finalStationDone)
        {
            Destroy(prod.obj);
            prod.isDestroyed = true;
            // stationOccupants[stIndex] boşalt
            if (stationOccupants[stIndex] == prod)
                stationOccupants[stIndex] = null;
            return;
        }

        // Hangi istasyonun Transform'u?
        Transform stationTf = stations[stIndex].transform;

        if (prod.allRotationsDone)
        {
            // İstasyona yaklaş
            Vector3 direction = stationTf.position - prod.obj.transform.position;
            float dist = moveSpeed * Time.deltaTime;
            if (direction.magnitude <= dist)
            {
                // Vardık
                prod.obj.transform.position = stationTf.position;

                // Bu istasyonda rotasyon daha yapılmadıysa
                if (!prod.rotationDoneThisStation)
                {
                    // StationScript'ten al
                    Vector3[] stationSteps = stations[stIndex].GetRotationSteps();
                    // stationSteps boş ise => ürün düz gider
                    if (stationSteps == null || stationSteps.Length == 0)
                    {
                        // Rotasyon yok => bitik say
                        prod.allRotationsDone = true;
                        prod.rotationDoneThisStation = true;

                        // Son station değilse bir sonraki
                        if (stIndex < stations.Length - 1)
                            GoToNextStation(prod);
                        else
                            prod.finalStationDone = true;
                    }
                    else
                    {
                        // Başlat
                        prod.currentRotationSteps = stationSteps;
                        prod.subRotationIndex = 0;
                        prod.allRotationsDone = false;
                        SetupRotationStep(prod, 0);
                    }
                }
                else
                {
                    // Zaten yapıldı => geç
                    if (stIndex < stations.Length - 1)
                        GoToNextStation(prod);
                    else
                        prod.finalStationDone = true;
                }
            }
            else
            {
                // Yaklaşmaya devam
                prod.obj.transform.Translate(direction.normalized * dist, Space.World);
            }
        }
        else
        {
            // Rotasyon sürüyor
            UpdateRotationStep(prod);
        }
    }

    private void SetupRotationStep(ProductData prod, int stepIndex)
    {
        if (prod.currentRotationSteps == null || stepIndex < 0 || stepIndex >= prod.currentRotationSteps.Length)
        {
            FinishAllRotations(prod);
            return;
        }
        prod.startRotation = prod.obj.transform.rotation;
        prod.endRotation = prod.startRotation * Quaternion.Euler(prod.currentRotationSteps[stepIndex]);
        prod.rotationTimer = rotationStepTime;
    }

    private void UpdateRotationStep(ProductData prod)
    {
        if (prod.subRotationIndex >= prod.currentRotationSteps.Length)
        {
            FinishAllRotations(prod);
            return;
        }

        if (prod.rotationTimer > 0f)
        {
            float t = 1f - (prod.rotationTimer / rotationStepTime);
            prod.obj.transform.rotation = Quaternion.Lerp(prod.startRotation, prod.endRotation, t);

            prod.rotationTimer -= Time.deltaTime;
            if (prod.rotationTimer <= 0f)
            {
                // Adım bitti
                prod.obj.transform.rotation = prod.endRotation;
                prod.subRotationIndex++;
                if (prod.subRotationIndex < prod.currentRotationSteps.Length)
                {
                    SetupRotationStep(prod, prod.subRotationIndex);
                }
                else
                {
                    FinishAllRotations(prod);
                }
            }
        }
    }

    private void FinishAllRotations(ProductData prod)
    {
        prod.allRotationsDone = true;
        prod.subRotationIndex = 0;
        prod.currentRotationSteps = null;
        prod.rotationDoneThisStation = true;

        // Eğer son istasyondaysa finalStationDone = true
        if (prod.currentStationIndex == stations.Length - 1)
        {
            prod.finalStationDone = true;
        }
        else
        {
            // Rotasyon bitince bir sonraki istasyona geçebilir
            GoToNextStation(prod);
        }
    }

    private void GoToNextStation(ProductData prod)
    {
        int oldIndex = prod.currentStationIndex;
        int newIndex = oldIndex + 1;
        if (newIndex >= stations.Length) return;

        if (stationOccupants[newIndex] == null)
        {
            stationOccupants[oldIndex] = null;
            stationOccupants[newIndex] = prod;
            prod.currentStationIndex = newIndex;
            prod.rotationDoneThisStation = false;

            // İSTASYON ADI
            var pScript = prod.obj.GetComponent<ProductScript>();
            if (pScript != null)
            {
                string stName = stations[newIndex].gameObject.name;
                pScript.SetStationName(stName);
            }
        }
        else
        {
            // dolu => bekle
        }
    }


    // (Opsiyonel) çizim
    private void OnDrawGizmos()
    {
        if (stations == null || stations.Length < 1) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] != null)
            {
                var stPos = stations[i].transform.position;
                Gizmos.DrawSphere(stPos, 0.2f);

                if (i < stations.Length - 1 && stations[i + 1] != null)
                {
                    var stNext = stations[i + 1].transform.position;
                    Gizmos.DrawLine(stPos, stNext);
                }
            }
        }
    }
}

/// <summary>
/// Her ürünle ilgili verileri tutar.
/// </summary>
[System.Serializable]
public class ProductData
{
    public GameObject obj;
    public int currentStationIndex = 0;
    public bool rotationDoneThisStation = false;
    public bool allRotationsDone = true;
    public bool finalStationDone = false;
    public bool isDestroyed = false;

    // Rotasyon adımları
    public Vector3[] currentRotationSteps;
    public int subRotationIndex;
    public float rotationTimer;
    public Quaternion startRotation;
    public Quaternion endRotation;

    public ProductData(GameObject o)
    {
        obj = o;
    }
}
