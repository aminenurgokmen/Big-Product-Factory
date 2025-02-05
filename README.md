Steps to Set Up the Waypoint System
Step 1: Assign the waypoints (stations) where the product should stop
Inside the WayPointObj, add the stations (or Transforms) where the product should stop.
Each station can have a StationScript component to define specific actions (rotations, animations, etc.).
Step 2: Assign your product prefab
Drag and drop your product prefab into the designated field in the PipelineManager script.
Step 3: Set the spawn count and speed
Define how many products should be spawned.
Adjust the movement speed of the products between stations.
Step 4: Configure rotations for each station
Select a station and navigate to its StationScript component in the Inspector.
Enter the rotation steps that the product should perform when it arrives at this station.
If no rotation is assigned, the product will move straight to the next station.
Additional Feature
🖱 Click on any product, and it will display which station it is currently at.

Each product knows its current station.
Clicking on a product will print its current station name in the console.
This setup allows for dynamic waypoint-based movement, with configurable rotations and custom actions per station, making it a flexible and reusable system. 🚀
