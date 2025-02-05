# **Waypoint System Setup Guide**

## **Step 1: Assign Waypoints (Stations)**
- Inside the **WayPointObj**, add the **stations** (or Transforms) where the product should stop.
- Each station can have a **StationScript** component to define specific actions (rotations, animations, etc.).

## **Step 2: Assign Your Product Prefab**
- Drag and drop your **product prefab** into the designated field in the **PipelineManager** script.

## **Step 3: Set the Spawn Count and Speed**
- Define how many products should be spawned.
- Adjust the movement speed of the products between stations.

## **Step 4: Configure Rotations for Each Station**
- Select a station and navigate to its **StationScript** component in the Inspector.
- Enter the **rotation steps** that the product should perform when it arrives at this station.
- If no rotation is assigned, the product will move straight to the next station.

---

## **Additional Feature**
### 🖱 **Click on Any Product to See Its Current Station**
- Each product knows its current station.
- Clicking on a product will print its current station name in the console.

This setup allows for **dynamic waypoint-based movement**, with configurable **rotations** and **custom actions** per station, making it a flexible and reusable system. 🚀

