# VRMultiplayer

Juego multijugador en red local (LAN) compatible con VR para visores Oculus en Windows.

Proyecto: `VRMultiplayer`
Versión actual: `0.1.0`
Unity target: `2020.3.12f1`
Plataforma: Windows (Standalone) + Oculus

## Notas de la versión 0.1.0

### Novedades
- Primera versión del proyecto: sesión multijugador en LAN con jugadores en VR.
- Red basada en UNet (`com.unity.multiplayer-hlapi`) con `NetworkManager` y `NetworkManagerHUD` para iniciar host, cliente o servidor.
- Soporte para 3 jugadores: 1 host y 2 clientes (`Max Connections = 2`).
- Prefab de jugador `XRRig` que sincroniza el rig (`NetworkTransform`) y la cabeza (`NetworkTransformChild` sobre `Main Camera`).
- `PlayerSetup` desactiva `Camera`, `AudioListener` y `TrackedPoseDriver` en los jugadores remotos, de modo que cada visor solo controla su propio avatar.
- Escena `Main` con 3 puntos de aparición (`NetworkStartPosition`) y entorno de prueba.
- Compatible con visores Oculus (Rift, o Quest / Quest 2 vía Link) mediante XR Plug-in Management y Oculus XR Plugin.

### Correcciones
- Ninguna (versión inicial).

### Mejoras
- Ninguna (versión inicial).

### Problemas conocidos
- La IP del host está fija en el `NetworkManager` (`Network Address = 192.168.8.7`); cada cliente debe cambiarla en el HUD o en el inspector antes de conectarse.
- El matchmaker de UNet (`mm.unet.unity3d.com`) ya no está en servicio; solo funciona la conexión directa por LAN.
- `NetworkManagerHUD` se dibuja con `OnGUI`, por lo que solo se ve en la ventana de escritorio y no dentro del visor.
- Solo se sincroniza la cabeza del jugador; aún no hay representación de mandos ni manos.
- La etiqueta `PlayerName` ("Soy un jugador") es estática: el campo `playerName` de `PlayerSetup` todavía no se usa ni se sincroniza.
- XR solo está configurado para Standalone (Windows); no hay configuración para Android (Quest sin PC).

### Obsolescencias
- El proyecto usa UNet (`com.unity.multiplayer-hlapi`), sistema de red deprecado por Unity. Se recomienda migrar a Netcode for GameObjects en versiones futuras.
- `CameraOffset` y `TrackedPoseDriver` provienen de `com.unity.xr.legacyinputhelpers`, componentes legacy de Unity XR.

## Dependencias

### Resueltas automáticamente por UPM
| Dependencia | Versión | Motivo |
| :--- | :--- | :--- |
| `com.unity.multiplayer-hlapi` | `1.1.1` | UNet: `NetworkManager`, `NetworkIdentity`, `NetworkTransform`, `NetworkTransformChild`, `NetworkBehaviour`. |
| `com.unity.xr.management` | `4.2.0` | Inicialización de XR al arrancar (XR Plug-in Management). |
| `com.unity.xr.oculus` | `1.9.1` | Loader y ajustes de Oculus (`Oculus Loader`, `Oculus Settings`). |
| `com.unity.xr.legacyinputhelpers` | `2.1.7` | `CameraOffset` y `TrackedPoseDriver` del `XRRig` (dependencia transitiva de `com.unity.xr.management`). |
| `com.unity.textmeshpro` | `3.0.6` | Etiqueta `PlayerName` (`TextMeshProUGUI`) sobre la cabeza del jugador. |
| `com.unity.ugui` | `1.0.0` | `Canvas` en World Space del jugador y `EventSystem` de la escena. |

### Instalación manual requerida
| Dependencia | Motivo |
| :--- | :--- |
| **Unity 2020.3.12f1** | Versión con la que se creó el proyecto; instalarla desde Unity Hub con el módulo *Windows Build Support*. |
| **App Oculus / Meta Quest Link (PC)** | Runtime de Oculus necesario en Windows para ejecutar el proyecto con visor (Rift, o Quest vía Link / Air Link). Se descarga desde el sitio de Meta. |

## Guía de implementación

### Módulos principales

#### 1. Red (UNet)
Gestiona la sesión host/cliente en la red local y el spawn de los jugadores.
* **Componentes principales:**
  * `NetworkManager`: puerto `7777`, `Network Address`, `Max Connections = 2`, `Player Prefab = XRRig` y `Auto Create Player` activo.
  * `NetworkManagerHUD`: botones *LAN Host*, *LAN Client* y *LAN Server Only* en la ventana de escritorio.
  * `NetworkStartPosition` (`SpawnPosition _1`, `_2`, `_3`): puntos de aparición de los jugadores (método aleatorio).
* **Pasos para la implementación:**
  1. Abrir la escena `Assets/Scenes/Main.unity`.
  2. En el equipo host, pulsar **LAN Host** en el HUD.
  3. En cada cliente, escribir la IP LAN del host en el campo del HUD y pulsar **LAN Client**.
  4. Permitir el puerto UDP `7777` en el firewall de Windows del host.

#### 2. Jugador VR (`XRRig`)
Prefab que representa a cada jugador en red y controla la cámara del jugador local.
* **Componentes principales:**
  * `NetworkIdentity`: con `Local Player Authority` para que el cliente propietario controle su transform.
  * `CameraOffset`: aplica la altura de la cámara (`1.36` m) al hijo `Camera Offset`.
  * `TrackedPoseDriver`: en `Main Camera`, sigue la pose del visor (ojo central).
  * `NetworkTransform`: sincroniza el transform raíz del rig (cada `0.1` s).
  * `NetworkTransformChild`: sincroniza `Main Camera`, es decir, la cabeza (cada `0.1` s).
  * `PlayerSetup`: desactiva los componentes de VR en los jugadores remotos.
  * `HeadModel` y `Canvas/PlayerName`: representación visual de la cabeza y etiqueta en World Space.
* **Pasos para la implementación:**
  1. Mantener `Assets/Prefabs/XRRig.prefab` asignado como `Player Prefab` del `NetworkManager`.
  2. Colocar como hijos de `Main Camera` los elementos visuales que deban seguir la cabeza y verse en los demás jugadores.
  3. Si se agregan componentes de cámara o de entrada al prefab, desactivarlos en `PlayerSetup.DisableVRComponents()` cuando `isLocalPlayer` sea falso.

#### 3. XR (Oculus)
Inicializa el visor al arrancar la aplicación en Windows.
* **Componentes principales:**
  * `XRGeneralSettings`: `Initialize XR on Startup` activo para Standalone.
  * `Oculus Loader`: único loader habilitado en la pestaña PC.
  * `Oculus Settings`: `Stereo Rendering Mode` en Multi Pass, targets Quest y Quest 2.
* **Pasos para la implementación:**
  1. Instalar la app Oculus / Meta Quest Link y conectar el visor (Link o Air Link).
  2. Verificar en `Project Settings > XR Plug-in Management` (pestaña PC) que `Oculus` esté habilitado.
  3. Compilar desde `File > Build Settings` para *PC, Mac & Linux Standalone* (Windows) con la escena `Main`.

### Componentes individuales (Utilities)
| Componente | Descripción | Propiedades clave / Caso de uso |
| :--- | :--- | :--- |
| **`PlayerSetup`** | `NetworkBehaviour` del `XRRig` que, en `Start`, desactiva cámara, audio y tracking si el jugador no es local. | `vrCmamera`, `audioListener`, `trackedPoseDriver`, `playerName` (reservado, sin uso). Evita que un jugador remoto tome el control de la cámara o del audio del visor local. |
