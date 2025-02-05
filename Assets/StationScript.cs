using UnityEngine;

/// <summary>
/// Her bir istasyon (waypoint) üzerinde duracak script.
/// Bu script üzerinden hangi rotasyon adımları yapılacağını Inspector'dan belirleyebilirsiniz.
/// Eğer rotationSteps boş veya devre dışıysa, product düz geçer.
/// </summary>
public class StationScript : MonoBehaviour
{
    [Header("Rotasyon Adımları")]
    [Tooltip("Bu istasyonda uygulanacak dönüşler. Her bir vektör (X, Y, Z) Euler açı farkını temsil eder.")]
    public Vector3[] rotationSteps;

    // Dilerseniz başka parametreler de ekleyebilirsiniz
    // Örneğin: public bool spawnSomething;
    // veya public float waitTime;
    // vb.

    /// <summary>
    /// Bu istasyon için tanımlanmış rotasyon adımlarını döndürür.
    /// Boş veya null ise ürün hiçbir şey yapmadan geçecek.
    /// </summary>
    public Vector3[] GetRotationSteps()
    {
        if (rotationSteps == null) return new Vector3[0];
        return rotationSteps;
    }
}
