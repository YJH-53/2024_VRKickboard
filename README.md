VRKickboard is a Unity-based electric-scooter (kickboard) simulator and driving-capability assessment environment. It includes arcade-style scooter physics, scenario/zone progression, scoring and violation detection, pedestrian/traffic interactions, and optional serial (Arduino) input for hardware controllers.


## Preview

<table>
	<tr>
		<td align="center" valign="middle">
			<figure>
				<img src="videos/scooter-moving-gif.gif" alt="scooter preview" width="420" />
				<figcaption style="text-align:center">Scooter preview</figcaption>
			</figure>
		</td>
		<td align="center" valign="middle">
			<figure>
				<img src="videos/sim-gif.gif" alt="simulation preview" width="560" />
				<figcaption style="text-align:center">Simulation preview</figcaption>
			</figure>
		</td>
	</tr>
</table>

---

## Quick facts

- Unity Editor: 2022.3.39f1 (see `ProjectSettings/ProjectVersion.txt`)
- Important packages (in `Packages/manifest.json`): TextMeshPro, XR Interaction Toolkit, XR Management, Oculus XR, Cinemachine, Postprocessing, Visual Scripting, Animation Rigging, Timeline

---

## Project layout (high level)

- `Assets/` : Unity assets, scenes, scripts, prefabs and plugins
	- `Assets/BikePhysics/Arcade Bike Physics/` : core gameplay and vehicle scripts (bike controller, speed monitor, scoring, traffic lights, zones, camera, UI)
	- `Assets/ArduinoSerial/` and `Assets/Ardity/` : serial communication helpers (SerialController, MessageListener, MessageWriter)
	- `Assets/Oculus/` : Oculus Integration and sample framework
- `Scenes/` : example scenes and town maps (e.g., `SampleScene.unity`, `Town_Zone_0909.unity`)
- `ProjectSettings/` and `Packages/` : config and package manifests

---

## How to open the project

1. Install Unity Hub and Unity Editor 2022.3.x (LTS). The project was tested with 2022.3.39f1.
2. From Unity Hub, Add the project folder (`d:/2024_VRKickboard`) and open it.
3. Wait for Unity to restore packages (Package Manager).
4. Open a scene (see recommended scenes below) and press Play.

If you plan to use VR with Oculus, install the Oculus PC runtime and enable the Oculus loader in Project Settings -> XR Plug-in Management.

---

## Quick facts

- Unity Editor: 2022.3.39f1 (see `ProjectSettings/ProjectVersion.txt`)
- Important packages (in `Packages/manifest.json`): TextMeshPro, XR Interaction Toolkit, XR Management, Oculus XR, Cinemachine, Postprocessing, Visual Scripting, Animation Rigging, Timeline

---

## Project layout (high level)

- `Assets/` : Unity assets, scenes, scripts, prefabs and plugins
	- `Assets/BikePhysics/Arcade Bike Physics/` : core gameplay and vehicle scripts (bike controller, speed monitor, scoring, traffic lights, zones, camera, UI)
	- `Assets/ArduinoSerial/` and `Assets/Ardity/` : serial communication helpers (SerialController, MessageListener, MessageWriter)
	- `Assets/Oculus/` : Oculus Integration and sample framework
- `Scenes/` : example scenes and town maps (e.g., `SampleScene.unity`, `Town_Zone_0909.unity`)
- `ProjectSettings/` and `Packages/` : config and package manifests

---

## How to open the project

1. Install Unity Hub and Unity Editor 2022.3.x (LTS). The project was tested with 2022.3.39f1.
2. From Unity Hub, Add the project folder (`d:/2024_VRKickboard`) and open it.
3. Wait for Unity to restore packages (Package Manager).
4. Open a scene (see recommended scenes below) and press Play.

If you plan to use VR with Oculus, install the Oculus PC runtime and enable the Oculus loader in Project Settings -> XR Plug-in Management.

---

## Recommended scenes to test

- `SampleScene.unity` — quick smoke test scene.
- `Town_0828.unity`, `Town_0827.unity`, `Town_Zone_0909.unity` — full town scenarios with traffic lights and pedestrians.
- `Town_HitAdd.unity` — scenarios with more collision cases.

Open any scene from the `Scenes/` folder and press Play.

---

## Controls

Two input modes are supported:

1) Keyboard (default, Editor):
	 - Horizontal: Arrow keys / A,D (mapped to Unity Input axis "Horizontal")
	 - Vertical: Arrow keys / W,S (mapped to Unity Input axis "Vertical")
	 - Space: used by PauseMenu/Start UI to start or restart scenarios

2) Arduino / Serial (hardware controller):
	 - `SerialController` reads lines from the configured COM port and forwards them to `MessageListener`.
	 - `MessageListener` expects lines in the format: value0/value1/value2/value3/value4
		 - These five numeric values are parsed and normalized into:
			 - `hall_b_normalizedValue` — brake (0..1)
			 - `hall_a_normalizedValue` — throttle (0..1)
			 - `roll`, `pitch` — IMU orientation floats
			 - `handle_normalizedValue` — steering (-1..1)
	 - Configure `SerialController.portName` (e.g., COM3) and `baudRate` (default 115200). Assign the `messageListener` GameObject in the Inspector.

When `MessageListener.isReady` is true, `ArcadeBikeController` reads inputs from its fields; otherwise it falls back to keyboard input.

---

## Major systems & important scripts

- ArcadeBikeController (Assets/BikePhysics/.../Scripts/Bike/ArcadeBikeController.cs)
	- Vehicle physics and input handling.
	- Detects traffic lights in view, computes red/green violation flags, handles zone counters (enterZoneX_Count), and drives visuals/audio.

- SpeedMonitor (Assets/BikePhysics/.../Scripts/Bike/SpeedMonitor.cs)
	- Tracks active zone, speed (km/h), overspeed/underspeed detection, collisions, and off-track recovery.

- ScoringSystem (Assets/BikePhysics/.../Scripts/ScoringSystem/ScoringSystem.cs)
	- Maintains a score (default 100) and deducts penalty points for violations (off-zone, speed violations, collisions, traffic light violations).
	- Updates UI TextMeshPro elements for score, warnings, and messages.

- TrafficLightController / TrafficPedLightController (Assets/BikePhysics/.../Scripts/TrafficLightControl/)
	- Manage light cycles, detect scooter presence in proximity, and set `isGreenLight`/`isRedLight` flags consumed by the bike controller.

- PauseMenu (Assets/BikePhysics/.../Scripts/ZoneExplanation/PauseMenu.cs)
	- Controls scenario flow, explanation/pause screens for zones, and start/end UI logic. Pauses the physics by zeroing velocities and setting Time.timeScale = 0.

- TakeDamage (Assets/BikePhysics/.../Scripts/DamageEffect/TakeDamage.cs)
	- Applies a PostProcessing vignette visual effect when collisions or violations occur.

- GeneratePlane (Assets/BikePhysics/.../Scripts/GeneratePlane/GeneratePlane.cs)
	- Creates adjacent ground plane tiles dynamically so the town can appear to extend as the player moves.

- ArrowManager (Assets/BikePhysics/.../Scripts/RoadArrow/ArrowManager.cs)
	- Activates in-front arrows to guide the player along the route.

- SerialController & MessageListener (Assets/ArduinoSerial and Assets/Ardity)
	- Threaded serial read/write helper and the message parser. Message format and normalization are controlled in `MessageListener.OnMessageArrived`.

---

## Running with Arduino (serial) — quick checklist

1. Connect your device and find the COM port (Windows Device Manager).
2. In Unity, select the `SerialController` GameObject and set `portName` to your COM port and `baudRate` to match your device.
3. Ensure `SerialController.messageListener` references the `MessageListener` GameObject in the scene.
4. The device must send newline-terminated lines such as:
	 512/480/1.23/0.12/300
	 (five numeric values separated by `/`).
5. Press Play; check the Console for MessageListener logs and `isReady=true`.

If you change the serial format, update `MessageListener.OnMessageArrived` accordingly.

---

## Troubleshooting

- Serial / connection issues:
	- Confirm COM port and baud rate. Ensure no other program (Arduino Serial Monitor, other tools) is using the COM port.
	- If messages arrive but parsing fails, verify the line format and separators.

- Missing references or NullReferenceExceptions:
	- Many scripts (ScoringSystem, SpeedMonitor, PauseMenu) expect UI Text or other components assigned in the Inspector. When opening a new scene, verify those references.

- VR/Oculus issues:
	- Ensure Oculus runtime is installed and XR Plug-in Management is configured. Use the Oculus documentation for runtime pairing.

- PostProcessing/vignette not visible:
	- Make sure the `PostProcessVolume` component exists on the object used by `TakeDamage` and that the PostProcessing package/profile contains Vignette settings.

---

## Extending the project

- To add a new driving scenario, copy an existing `Town_*` scene, modify roads/pedestrians/traffic lights, and assign the correct zone `tag`s (Zone0...Zone7). The `SpeedMonitor` uses zone tags to track progression.
- To change scoring rules, edit `ScoringSystem` penalty values and thresholds.
- To support different hardware, modify `MessageListener` parsing and normalizing functions.

---

## Credits & licenses

- Project code and assets in this repository.
- Ardity / Serial helpers (SerialController, MessageListener) include headers referencing Creative Commons Attribution — see source file headers in `Assets/Ardity` and `Assets/ArduinoSerial`.
- Oculus and Unity packages included are subject to their respective licenses.

---

Last updated: 2025-10-04
