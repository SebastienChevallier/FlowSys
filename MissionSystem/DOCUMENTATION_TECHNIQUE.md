# DOCUMENTATION TECHNIQUE - SYSTÈME DE MISSIONS
**Version 1.0 - Unity Project ADIV_SST**

---

## TABLE DES MATIÈRES

1. [Architecture du Système](#1-architecture-du-système)
2. [Outils de Base](#2-outils-de-base)
3. [How To - Guides Pratiques](#3-how-to---guides-pratiques)
4. [Améliorations Possibles](#4-améliorations-possibles)

---

## 1. ARCHITECTURE DU SYSTÈME

### 1.1 Vue d'Ensemble

Le système de missions est construit sur une **architecture modulaire SOLID** utilisant le **pattern State Machine** pour gérer le flux d'exécution. Il sépare strictement :

- **Core** : Logique métier et orchestration
- **Data** : Configuration via ScriptableObjects
- **Services** : Intégrations avec les systèmes existants (VORTA/EVAVEO)
- **Actions/Conditions** : Composants extensibles

```
┌─────────────────────────────────────────────────────────────┐
│                      MISSION MANAGER                         │
│                      (Singleton/Context)                     │
└──────────────┬──────────────────────────────────────────────┘
               │
       ┌───────┴────────┐
       │                │
┌──────▼──────┐  ┌─────▼──────┐
│   State     │  │  Services  │
│  Machine    │  │  Injection │
└──────┬──────┘  └────────────┘
       │
┌──────▼──────────────────────┐
│    Mission Step State       │
│  ┌────────────────────────┐ │
│  │  onEnter Actions       │ │
│  │  (Sequential Exec)     │ │
│  └────────────────────────┘ │
│  ┌────────────────────────┐ │
│  │  Exit Conditions       │ │
│  │  (Evaluated in Update) │ │
│  └────────────────────────┘ │
│  ┌────────────────────────┐ │
│  │  onExit Actions        │ │
│  └────────────────────────┘ │
└─────────────────────────────┘
```

### 1.2 Composants Core

#### **MissionManager** (Singleton)
**Fichier** : `Core/MissionManager.cs`

**Responsabilités** :
- Orchestration globale du système
- Implémentation de `IMissionContext` (fournit les services)
- Gestion du chargement des scènes
- Progression entre les étapes
- Point d'entrée unique : `StartMission(MissionConfigSO)`

**Services Injectés** :
```csharp
[SerializeField] private MissionSceneLoader sceneLoader;
[SerializeField] private VortaPlayerTeleporter playerTeleporter;
[SerializeField] private MissionObjectRegistry objectRegistry;
```

**Cycle de Vie** :
```
StartMission()
    ↓
LoadScenes (Art + Interaction)
    ↓
OnScenesLoaded()
    ↓
TeleportPlayer()
    ↓
ResetUserActions()
    ↓
PlayMissionStartVoiceOver()
    ↓
StartNextStep() → Loop
    ↓
OnMissionComplete()
```

#### **MissionStateMachine**
**Fichier** : `Core/MissionStateMachine.cs`

Pattern State classique :
```csharp
public void ChangeState(IMissionState newState)
{
    currentState?.OnExit(context);
    currentState = newState;
    currentState?.OnEnter(context);
}

public void Update()
{
    currentState?.Update(context);
}
```

#### **MissionStepState** (État Concret)
**Fichier** : `Core/MissionStepState.cs`

**Responsabilités** :
- Exécution séquentielle des `onEnterActions`
- Configuration des `userActionToggles`
- Évaluation continue des `exitConditions`
- Exécution des `onExitActions`

**Flux d'Exécution** :
```
OnEnter()
    ↓
ExecuteNextAction() [Récursif]
    ├─ Action Sync → Execute() → Next
    └─ Action Async → ExecuteAsync() → Wait Callback → Next
    ↓
OnActionsCompleted()
    ├─ Configure UserActionToggles
    └─ Play StepVoiceOver
    ↓
Update() [Chaque frame]
    └─ AllConditionsMet() ?
        └─ Yes → OnStepComplete Event
    ↓
OnExit()
    └─ Execute onExitActions
```

#### **IMissionContext** (Interface)
**Fichier** : `Core/IMissionContext.cs`

Contrat de services disponibles pour Actions/Conditions :
```csharp
public interface IMissionContext
{
    MissionConfigSO CurrentMission { get; }
    MissionStepConfigSO CurrentStep { get; }
    
    GameObject GetObjectById(string id);
    void TeleportPlayer(Vector3 position, Vector3 rotation);
    void PlayVoiceOver(AudioClip clip, Action onComplete = null);
    void PlayVoiceOverByKey(string voiceOverKey, Action onComplete = null);
    
    UserActionsManager UserActionsManager { get; }
    
    Coroutine StartCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
}
```

**Principe** : Dependency Inversion (SOLID) - Les actions dépendent de l'abstraction, pas du MissionManager concret.

### 1.3 Structure de Données

#### **MissionConfigSO** (ScriptableObject)
**Fichier** : `Data/MissionConfigSO.cs`

```csharp
public class MissionConfigSO : ScriptableObject
{
    // Identification
    public string missionId;
    public string missionName;
    public EMissionMode missionMode; // Formation/Evaluation
    
    // Scènes
    [Scene] public string sceneArt;
    [Scene] public string sceneInteraction;
    
    // Spawn
    public Vector3 playerSpawnPosition;
    public Vector3 playerSpawnRotation;
    
    // Steps
    public List<MissionStepConfigSO> steps;
    
    // Audio
    [VoiceOverSelector] public string missionStartVoiceOverKey;
    [VoiceOverSelector] public string missionCompleteVoiceOverKey;
}
```

#### **MissionStepConfigSO** (ScriptableObject)
**Fichier** : `Data/MissionStepConfigSO.cs`

```csharp
public class MissionStepConfigSO : ScriptableObject
{
    // Identification
    public string stepId;
    public string stepName;
    
    // Actions
    public List<StepActionSO> onEnterActions;
    public List<StepActionSO> onExitActions;
    
    // Conditions
    public List<StepConditionSO> exitConditions; // AND logic
    
    // UserActions
    public List<UserActionToggle> userActionToggles;
    
    // Audio
    [VoiceOverSelector] public string stepVoiceOverKey;
}
```

### 1.4 Système d'Actions

#### **StepActionSO** (Classe Abstraite)
**Fichier** : `Data/StepActionSO.cs`

```csharp
public abstract class StepActionSO : ScriptableObject
{
    public bool isAsync = false;
    
    // Synchrone (legacy)
    public virtual void Execute(IMissionContext context)
    {
        ExecuteAsync(context, onComplete: null);
    }
    
    // Asynchrone (standard)
    public abstract void ExecuteAsync(IMissionContext context, Action onComplete);
    
    public virtual string GetActionName() { ... }
}
```

**Principe** : Open/Closed (SOLID) - Ouvert à l'extension, fermé à la modification.

#### **Actions Synchrones** (Exécution Immédiate)

| Action | Fichier | Description |
|--------|---------|-------------|
| `ShowObjectActionSO` | `Actions/Sync/ShowObjectActionSO.cs` | Active un GameObject par ID |
| `HideObjectActionSO` | `Actions/Sync/HideObjectActionSO.cs` | Désactive un GameObject par ID |
| `ShowGhostActionSO` | `Actions/Sync/ShowGhostActionSO.cs` | Affiche un ghost trace |
| `HideGhostActionSO` | `Actions/Sync/HideGhostActionSO.cs` | Cache un ghost trace |
| `EnableUserActionActionSO` | `Actions/Sync/EnableUserActionActionSO.cs` | Active une UserAction |
| `DisableUserActionActionSO` | `Actions/Sync/DisableUserActionActionSO.cs` | Désactive une UserAction |
| `TeleportPlayerActionSO` | `Actions/Sync/TeleportPlayerActionSO.cs` | Téléporte le joueur |

**Exemple d'Implémentation** :
```csharp
public override void ExecuteAsync(IMissionContext context, Action onComplete)
{
    GameObject obj = context.GetObjectById(objectId);
    if (obj != null)
    {
        obj.SetActive(true);
        Debug.Log($"[MissionSystem] Showed object: {objectId}");
    }
    onComplete?.Invoke(); // Immédiat
}
```

#### **Actions Asynchrones** (Avec Callback)

| Action | Fichier | Description |
|--------|---------|-------------|
| `PlayVoiceOverActionSO` | `Actions/Async/PlayVoiceOverActionSO.cs` | Joue une voix off (attend si `waitForCompletion=true`) |
| `WaitForUserActionActionSO` | `Actions/Async/WaitForUserActionActionSO.cs` | Attend qu'une UserAction soit exécutée |
| `WaitForObjectPickedActionSO` | `Actions/Async/WaitForObjectPickedActionSO.cs` | Attend la sélection d'un objet |
| `DelayActionSO` | `Actions/Async/DelayActionSO.cs` | Attend X secondes |
| `SequenceActionSO` | `Actions/Async/SequenceActionSO.cs` | Exécute plusieurs actions en séquence |

**Exemple d'Implémentation** :
```csharp
public override void ExecuteAsync(IMissionContext context, Action onComplete)
{
    UserAction ua = UserActionsManager.GetUserAction(userActionId);
    
    UnityAction<object> handler = null;
    handler = (data) =>
    {
        ua.onFire.RemoveListener(handler);
        onComplete?.Invoke(); // Callback différé
    };
    
    ua.onFire.AddListener(handler);
}
```

### 1.5 Système de Conditions

#### **StepConditionSO** (Classe Abstraite)
**Fichier** : `Data/StepConditionSO.cs`

```csharp
public abstract class StepConditionSO : ScriptableObject
{
    public abstract bool Evaluate(IMissionContext context);
    public virtual string GetConditionName() { ... }
}
```

#### **Conditions Disponibles**

| Condition | Fichier | Description |
|-----------|---------|-------------|
| `UserActionDoneConditionSO` | `Conditions/UserActionDoneConditionSO.cs` | Vérifie si une UserAction est complétée |
| `AllUserActionsDoneConditionSO` | `Conditions/AllUserActionsDoneConditionSO.cs` | Vérifie si toutes les UserActions sont complétées |
| `MissionModeConditionSO` | `Conditions/MissionModeConditionSO.cs` | Vérifie le mode (Formation/Évaluation) |
| `TimerConditionSO` | `Conditions/TimerConditionSO.cs` | Vérifie le temps écoulé |
| `ObjectActiveConditionSO` | `Conditions/ObjectActiveConditionSO.cs` | Vérifie l'état actif/inactif d'un objet |

**Logique d'Évaluation** :
```csharp
private bool AllConditionsMet(IMissionContext context)
{
    // AND logic - toutes doivent être vraies
    foreach (var condition in stepConfig.exitConditions)
    {
        if (!condition.Evaluate(context))
            return false;
    }
    return true;
}
```

### 1.6 Services et Intégrations

#### **MissionObjectRegistry**
**Fichier** : `Services/Implementations/MissionObjectRegistry.cs`

Registre d'objets accessibles par ID string :
```csharp
public GameObject GetObject(string id)
{
    // Recherche dans le registre
    // Utilisé par ShowObject, HideObject, etc.
}
```

**Setup** : Ajouter `MissionObjectRegistrar` sur les GameObjects importants.

#### **MissionSceneLoader**
**Fichier** : `Services/Implementations/MissionSceneLoader.cs`

Charge les scènes de mission avec écran de chargement :
```csharp
public void LoadMissionScenes(string sceneArt, string sceneInteraction, Action onComplete)
{
    // Utilise ScenesManager (EVAVEO)
}
```

#### **VortaPlayerTeleporter**
**Fichier** : `Services/Implementations/VortaPlayerTeleporter.cs`

Téléportation du joueur :
```csharp
public void Teleport(Vector3 position, Vector3 rotation)
{
    // Intégration avec VORTA
}
```

#### **Intégration VoixOffManager**

Le système utilise des **clés string** pour référencer les voix off :
```csharp
context.PlayVoiceOverByKey("Mission_Intro", onComplete: () => { ... });
```

**Avantages** :
- Gestion centralisée dans `PLS_VoiceOverData`
- Support multilingue automatique (_fr, _eng)
- Pas de références directes aux AudioClip
- Synchronisation Google Drive possible

### 1.7 Principes SOLID Appliqués

| Principe | Application |
|----------|-------------|
| **S**ingle Responsibility | MissionManager orchestre, MissionStepState exécute, Services gèrent leurs domaines |
| **O**pen/Closed | Actions/Conditions extensibles sans modifier le core |
| **L**iskov Substitution | Toutes les Actions/Conditions sont interchangeables |
| **I**nterface Segregation | IMissionContext expose uniquement ce qui est nécessaire |
| **D**ependency Inversion | Actions dépendent de IMissionContext, pas de MissionManager |

---

## 2. OUTILS DE BASE

### 2.1 Outils Éditeur

#### **Mission Manager Window**
**Menu** : `GAME > Mission System > Mission Manager Window`

**Fonctionnalités** :
- Création rapide de missions avec structure de dossiers automatique
- Génération de `MissionConfigSO` + dossier `Steps/`
- Nomenclature standardisée

**Utilisation** :
1. Entrer le nom de la mission (ex: "Mission_Boucherie")
2. Cliquer "Create Mission with Folder Structure"
3. La mission est créée dans `Assets/GAME/Missions/[NomMission]/`

#### **Custom Inspectors**

**MissionConfigSOEditor** :
- Bouton **"Create New Step"** : Crée un step dans le bon dossier
- Validation des scènes
- Prévisualisation des steps

**MissionStepConfigSOEditor** :
- Bouton **"Add Action"** : Menu contextuel avec toutes les actions
- Bouton **"Add Condition"** : Menu contextuel avec toutes les conditions
- Création automatique des assets dans `Actions/` et `Conditions/`
- Affichage des noms personnalisés (`GetActionName()`, `GetConditionName()`)

#### **Menu Items**

**GameObject > GAME > Mission System > Mission Manager** :
- Crée un GameObject avec MissionManager + tous les services
- Configuration automatique des références

**GameObject > GAME > Mission System > Mission Object Registrar** :
- Ajoute le composant `MissionObjectRegistrar` sur l'objet sélectionné
- Permet de définir un `objectId` pour référencement

### 2.2 Attributs Custom

#### **[VoiceOverSelector]**
Affiche un dropdown avec toutes les clés disponibles dans `PLS_VoiceOverData`.

```csharp
[VoiceOverSelector]
public string missionStartVoiceOverKey;
```

#### **[Scene]**
Affiche un sélecteur de scène dans l'Inspector.

```csharp
[Scene]
public string sceneArt;
```

### 2.3 Helpers

#### **IPickable / PickableObject**
**Fichier** : `Helpers/PickableObject.cs`

Interface pour les objets sélectionnables :
```csharp
public interface IPickable
{
    event Action<GameObject> OnPicked;
    void Pick();
}
```

Utilisé par `WaitForObjectPickedActionSO`.

#### **MissionObjectRegistrar**
**Fichier** : `Helpers/MissionObjectRegistrar.cs`

Composant à ajouter sur les GameObjects importants :
```csharp
public class MissionObjectRegistrar : MonoBehaviour
{
    public string objectId;
    
    void Awake()
    {
        MissionObjectRegistry.Register(objectId, gameObject);
    }
}
```

### 2.4 Logs et Debugging

Tous les logs sont préfixés par `[MissionSystem]` :
```
[MissionSystem] Starting mission: Mission_Boucherie
[MissionSystem] Scenes loaded
[MissionSystem] Entering step: Introduction
[MissionSystem] Executing action: PlayVoiceOver (Async: True)
[MissionSystem] Action completed: PlayVoiceOver
[MissionSystem] All exit conditions met for step: Introduction
[MissionSystem] Step completed: Introduction
[MissionSystem] Mission completed: Mission_Boucherie
```

**Activer la console** pour suivre l'exécution en temps réel.

---

## 3. HOW TO - GUIDES PRATIQUES

### 3.1 Créer une Mission Complète

#### **Étape 1 : Créer la Mission**

1. Menu : `GAME > Mission System > Mission Manager Window`
2. Entrer : "Mission_Boucherie"
3. Cliquer : "Create Mission with Folder Structure"

**Résultat** :
```
Assets/GAME/Missions/Mission_Boucherie/
├── Mission_Boucherie.asset (MissionConfigSO)
└── Steps/
```

#### **Étape 2 : Configurer la Mission**

Sélectionner `Mission_Boucherie.asset` :

```
Mission Information:
├── missionId: "mission_boucherie_001"
├── missionName: "Formation Boucherie"
└── missionMode: Formation

Scenes:
├── sceneArt: "Scene_Boucherie_Art"
└── sceneInteraction: "Scene_Boucherie_Interaction"

Player Spawn:
├── playerSpawnPosition: (0, 0, 5)
└── playerSpawnRotation: (0, 180, 0)

Audio:
├── missionStartVoiceOverKey: "Mission_Boucherie_Intro"
└── missionCompleteVoiceOverKey: "Mission_Boucherie_Complete"
```

#### **Étape 3 : Créer les Steps**

Cliquer **"Create New Step"** dans l'Inspector de la mission.

**Step 1 : Introduction**
```
Step Information:
├── stepId: "step_01_intro"
└── stepName: "Introduction"

onEnter Actions:
├── PlayVoiceOver
│   ├── voiceOverKey: "Boucherie_Bienvenue"
│   └── waitForCompletion: true
├── ShowObject
│   └── objectId: "Couteau"
└── WaitForObjectPicked
    └── objectId: "Couteau"

Exit Conditions:
└── UserActionDone
    └── userActionId: "PRENDRE_COUTEAU"

Audio:
└── stepVoiceOverKey: "Mission_Boucherie_Step01"
```

**Step 2 : Découpe**
```
Step Information:
├── stepId: "step_02_decoupe"
└── stepName: "Découpe de la viande"

onEnter Actions:
├── ShowObject
│   └── objectId: "Planche"
└── EnableUserAction
    └── actionId: "COUPER_VIANDE"

Exit Conditions:
└── UserActionDone
    └── userActionId: "COUPER_VIANDE"

User Actions Configuration:
└── userActionToggles:
    ├── COUPER_VIANDE: enabled
    └── PRENDRE_COUTEAU: disabled
```

#### **Étape 4 : Préparer la Scène**

**Créer le MissionManager** :
1. Menu : `GameObject > GAME > Mission System > Mission Manager`
2. Vérifier les références des services

**Enregistrer les Objets** :
1. Sélectionner le GameObject "Couteau"
2. Menu : `GameObject > GAME > Mission System > Mission Object Registrar`
3. Définir `objectId = "Couteau"`
4. Ajouter `PickableObject` si nécessaire
5. Répéter pour "Planche", etc.

#### **Étape 5 : Lancer la Mission**

**Option A : Script de démarrage**
```csharp
public class MissionStarter : MonoBehaviour
{
    [SerializeField] private MissionConfigSO missionToStart;
    
    void Start()
    {
        MissionManager.Instance.StartMission(missionToStart);
    }
}
```

**Option B : Menu VR**
Intégration avec `EvaveoMenuVR` pour sélection de missions.

### 3.2 Créer une Action Custom

**Exemple** : Action pour faire vibrer le contrôleur

#### **Étape 1 : Créer le Fichier**
`Actions/Sync/VibrateControllerActionSO.cs`

```csharp
using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [CreateAssetMenu(
        fileName = "Action_VibrateController", 
        menuName = "GAME/Mission System/Actions/Sync/Vibrate Controller", 
        order = 110)]
    public class VibrateControllerActionSO : StepActionSO
    {
        [Header("Vibration Settings")]
        public float intensity = 0.5f;
        public float duration = 0.2f;
        
        public VibrateControllerActionSO()
        {
            isAsync = false; // Synchrone
        }
        
        public override void ExecuteAsync(IMissionContext context, Action onComplete)
        {
            // Votre logique de vibration
            // Ex: XRController.SendHapticImpulse(intensity, duration);
            
            Debug.Log($"[MissionSystem] Vibrating controller: {intensity} for {duration}s");
            
            onComplete?.Invoke();
        }
        
        public override string GetActionName()
        {
            return $"Vibrate ({intensity})";
        }
    }
}
```

#### **Étape 2 : Utiliser l'Action**
1. Ouvrir un `MissionStepConfigSO`
2. Cliquer "Add Action"
3. Sélectionner "Vibrate Controller" dans le menu
4. Configurer `intensity` et `duration`

### 3.3 Créer une Condition Custom

**Exemple** : Condition pour vérifier la distance du joueur

#### **Étape 1 : Créer le Fichier**
`Conditions/PlayerDistanceConditionSO.cs`

```csharp
using UnityEngine;

namespace GAME.MissionSystem
{
    [CreateAssetMenu(
        fileName = "Condition_PlayerDistance", 
        menuName = "GAME/Mission System/Conditions/Player Distance", 
        order = 310)]
    public class PlayerDistanceConditionSO : StepConditionSO
    {
        [Header("Distance Settings")]
        public string targetObjectId;
        public float maxDistance = 2.0f;
        
        public override bool Evaluate(IMissionContext context)
        {
            GameObject target = context.GetObjectById(targetObjectId);
            if (target == null) return false;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return false;
            
            float distance = Vector3.Distance(player.transform.position, target.transform.position);
            return distance <= maxDistance;
        }
        
        public override string GetConditionName()
        {
            return $"Player < {maxDistance}m from '{targetObjectId}'";
        }
    }
}
```

#### **Étape 2 : Utiliser la Condition**
1. Ouvrir un `MissionStepConfigSO`
2. Cliquer "Add Condition"
3. Sélectionner "Player Distance"
4. Configurer `targetObjectId` et `maxDistance`

### 3.4 Créer une Action Asynchrone Complexe

**Exemple** : Attendre que le joueur regarde un objet

```csharp
using System;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    [CreateAssetMenu(
        fileName = "Action_WaitForPlayerLook", 
        menuName = "GAME/Mission System/Actions/Async/Wait For Player Look", 
        order = 210)]
    public class WaitForPlayerLookActionSO : StepActionSO
    {
        [Header("Look Settings")]
        public string targetObjectId;
        public float lookDuration = 2.0f;
        public float maxAngle = 30.0f;
        
        public WaitForPlayerLookActionSO()
        {
            isAsync = true; // Asynchrone
        }
        
        public override void ExecuteAsync(IMissionContext context, Action onComplete)
        {
            GameObject target = context.GetObjectById(targetObjectId);
            if (target == null)
            {
                Debug.LogError($"[MissionSystem] Target '{targetObjectId}' not found");
                onComplete?.Invoke();
                return;
            }
            
            context.StartCoroutine(WaitForLookCoroutine(target, onComplete));
        }
        
        private IEnumerator WaitForLookCoroutine(GameObject target, Action onComplete)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Camera playerCamera = Camera.main;
            
            float lookTime = 0f;
            
            while (lookTime < lookDuration)
            {
                Vector3 directionToTarget = (target.transform.position - playerCamera.transform.position).normalized;
                float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
                
                if (angle <= maxAngle)
                {
                    lookTime += Time.deltaTime;
                }
                else
                {
                    lookTime = 0f;
                }
                
                yield return null;
            }
            
            Debug.Log($"[MissionSystem] Player looked at '{targetObjectId}' for {lookDuration}s");
            onComplete?.Invoke();
        }
        
        public override string GetActionName()
        {
            return $"Wait Look '{targetObjectId}' ({lookDuration}s)";
        }
    }
}
```

### 3.5 Débugger une Mission

#### **Problème : Une étape ne se termine pas**

**Checklist** :
1. Vérifier les logs : `[MissionSystem] All exit conditions met` n'apparaît pas
2. Vérifier que toutes les `exitConditions` sont configurées
3. Tester chaque condition individuellement :
   ```csharp
   // Dans la console Unity
   var condition = stepConfig.exitConditions[0];
   Debug.Log(condition.Evaluate(MissionManager.Instance));
   ```
4. Vérifier que les UserActions sont bien déclenchées
5. Vérifier que les objets ont les bons `objectId`

#### **Problème : Une action ne s'exécute pas**

**Checklist** :
1. Vérifier les logs : `[MissionSystem] Executing action: [NomAction]`
2. Vérifier que l'action n'est pas `null` dans la liste
3. Pour les actions async, vérifier que `onComplete?.Invoke()` est appelé
4. Vérifier les références (objectId, userActionId, etc.)

#### **Problème : Les scènes ne se chargent pas**

**Checklist** :
1. Vérifier que les scènes sont dans Build Settings
2. Vérifier les noms exacts (case-sensitive)
3. Vérifier que `MissionSceneLoader` est assigné dans MissionManager
4. Vérifier les logs de `ScenesManager`

### 3.6 Optimisation et Bonnes Pratiques

#### **Performance**

**Actions** :
- Préférer les actions synchrones quand possible
- Éviter les `Update()` dans les actions
- Utiliser des coroutines pour les attentes longues

**Conditions** :
- `Evaluate()` est appelé chaque frame → optimiser
- Cacher les résultats si possible
- Éviter les `FindObjectOfType()` répétés

**Exemple d'Optimisation** :
```csharp
public class OptimizedConditionSO : StepConditionSO
{
    private GameObject cachedObject;
    private float lastCheckTime;
    private bool lastResult;
    
    public override bool Evaluate(IMissionContext context)
    {
        // Cache pour 0.1s
        if (Time.time - lastCheckTime < 0.1f)
            return lastResult;
        
        if (cachedObject == null)
            cachedObject = context.GetObjectById(objectId);
        
        lastResult = cachedObject != null && cachedObject.activeSelf;
        lastCheckTime = Time.time;
        
        return lastResult;
    }
}
```

#### **Organisation**

**Nomenclature** :
```
Missions/
├── Mission_[NomMission]/
│   ├── Mission_[NomMission].asset
│   ├── Steps/
│   │   ├── Step_01_[NomEtape].asset
│   │   ├── Step_02_[NomEtape].asset
│   │   └── ...
│   ├── Actions/
│   │   ├── Action_[Type]_[Description].asset
│   │   └── ...
│   └── Conditions/
│       ├── Condition_[Type]_[Description].asset
│       └── ...
```

**Réutilisation** :
- Créer des actions/conditions génériques dans un dossier `Common/`
- Utiliser `SequenceActionSO` pour combiner des actions réutilisables

#### **Testing**

**Mode Formation vs Évaluation** :
```csharp
// Utiliser MissionModeConditionSO pour des comportements différents
if (missionMode == EMissionMode.Formation)
{
    // Afficher des aides, ghosts, etc.
}
else
{
    // Mode évaluation : pas d'aide
}
```

**Tests Unitaires** :
```csharp
[Test]
public void TestUserActionDoneCondition()
{
    var condition = ScriptableObject.CreateInstance<UserActionDoneConditionSO>();
    condition.userActionId = "TEST_ACTION";
    
    // Setup mock context
    var mockContext = new MockMissionContext();
    
    Assert.IsFalse(condition.Evaluate(mockContext));
    
    // Simulate action done
    UserActionsManager.MarkActionDone("TEST_ACTION");
    
    Assert.IsTrue(condition.Evaluate(mockContext));
}
```

---

## 4. AMÉLIORATIONS POSSIBLES

### 4.1 Fonctionnalités Système

#### **4.1.1 Système de Sauvegarde**

**Objectif** : Sauvegarder la progression de la mission

**Implémentation** :
```csharp
[Serializable]
public class MissionSaveData
{
    public string missionId;
    public int currentStepIndex;
    public List<string> completedUserActions;
    public float elapsedTime;
}

public class MissionSaveSystem
{
    public void SaveMission(MissionManager manager)
    {
        var saveData = new MissionSaveData
        {
            missionId = manager.CurrentMission.missionId,
            currentStepIndex = manager.CurrentStepIndex,
            completedUserActions = UserActionsManager.GetCompletedActions(),
            elapsedTime = Time.time
        };
        
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString($"Mission_{saveData.missionId}", json);
    }
    
    public void LoadMission(string missionId)
    {
        string json = PlayerPrefs.GetString($"Mission_{missionId}");
        var saveData = JsonUtility.FromJson<MissionSaveData>(json);
        
        // Restaurer l'état
        MissionManager.Instance.LoadFromSave(saveData);
    }
}
```

**Intégration** :
- Sauvegarder automatiquement à chaque fin d'étape
- Bouton "Reprendre" dans le menu

#### **4.1.2 Système de Scoring**

**Objectif** : Évaluer la performance du joueur

**Implémentation** :
```csharp
public class MissionScoreTracker
{
    public int baseScore = 1000;
    public int currentScore;
    public int errorsCount;
    public float completionTime;
    
    public void OnStepComplete(MissionStepConfigSO step)
    {
        // Bonus temps
        if (completionTime < step.targetTime)
            currentScore += 100;
        
        // Malus erreurs
        currentScore -= errorsCount * 50;
    }
    
    public void OnUserActionError(string actionId)
    {
        errorsCount++;
        Debug.Log($"[Scoring] Error on {actionId}. Total errors: {errorsCount}");
    }
}
```

**Nouvelles Conditions** :
- `ScoreThresholdConditionSO` : Vérifie si le score est suffisant
- `MaxErrorsConditionSO` : Vérifie le nombre d'erreurs

#### **4.1.3 Système de Hints**

**Objectif** : Aider le joueur bloqué

**Implémentation** :
```csharp
public class HintActionSO : StepActionSO
{
    public string hintVoiceOverKey;
    public float delayBeforeHint = 30f;
    
    public override void ExecuteAsync(IMissionContext context, Action onComplete)
    {
        context.StartCoroutine(HintCoroutine(context, onComplete));
    }
    
    private IEnumerator HintCoroutine(IMissionContext context, Action onComplete)
    {
        yield return new WaitForSeconds(delayBeforeHint);
        
        // Vérifier si l'étape est toujours active
        if (context.CurrentStep == stepConfig)
        {
            context.PlayVoiceOverByKey(hintVoiceOverKey);
        }
        
        onComplete?.Invoke();
    }
}
```

**Utilisation** :
- Ajouter `HintActionSO` dans `onEnterActions` avec `isAsync=true`
- Le hint se joue si le joueur reste bloqué

#### **4.1.4 Système de Branches**

**Objectif** : Missions non-linéaires avec choix

**Implémentation** :
```csharp
public class BranchStepConfigSO : MissionStepConfigSO
{
    [System.Serializable]
    public class Branch
    {
        public string branchName;
        public List<StepConditionSO> conditions; // OR logic
        public MissionStepConfigSO nextStep;
    }
    
    public List<Branch> branches;
    
    public MissionStepConfigSO GetNextStep(IMissionContext context)
    {
        foreach (var branch in branches)
        {
            if (AnyConditionMet(branch.conditions, context))
                return branch.nextStep;
        }
        
        return null; // Default path
    }
}
```

**Exemple** :
```
Step: "Choix de méthode"
├── Branch A: "Méthode traditionnelle"
│   └── Condition: UserActionDone("CHOISIR_TRADITIONNEL")
│   └── Next: Step_Traditionnel
└── Branch B: "Méthode moderne"
    └── Condition: UserActionDone("CHOISIR_MODERNE")
    └── Next: Step_Moderne
```

#### **4.1.5 Système de Replay**

**Objectif** : Rejouer les actions du joueur

**Implémentation** :
```csharp
public class MissionRecorder
{
    [Serializable]
    public class RecordedAction
    {
        public float timestamp;
        public string actionId;
        public Vector3 position;
        public Quaternion rotation;
    }
    
    private List<RecordedAction> recording = new List<RecordedAction>();
    
    public void RecordAction(string actionId, Transform transform)
    {
        recording.Add(new RecordedAction
        {
            timestamp = Time.time,
            actionId = actionId,
            position = transform.position,
            rotation = transform.rotation
        });
    }
    
    public IEnumerator PlayRecording(GameObject replayAvatar)
    {
        float startTime = Time.time;
        
        foreach (var action in recording)
        {
            yield return new WaitUntil(() => Time.time - startTime >= action.timestamp);
            
            replayAvatar.transform.position = action.position;
            replayAvatar.transform.rotation = action.rotation;
            
            // Trigger action
            UserActionsManager.TriggerAction(action.actionId);
        }
    }
}
```

### 4.2 Nouvelles Actions

#### **4.2.1 Actions UI**

```csharp
// ShowUIMessageActionSO
public string messageKey;
public float displayDuration;

// HighlightObjectActionSO
public string objectId;
public Color highlightColor;
public float pulseSpeed;

// ShowArrowToObjectActionSO
public string targetObjectId;
public bool followPlayer;
```

#### **4.2.2 Actions Audio**

```csharp
// PlaySoundEffectActionSO
public AudioClip soundEffect;
public Vector3 position;
public bool is3D;

// PlayAmbientMusicActionSO
public AudioClip musicClip;
public float fadeInDuration;
public float fadeOutDuration;

// StopAllAudioActionSO
public bool fadeOut;
```

#### **4.2.3 Actions Animation**

```csharp
// PlayAnimationActionSO
public string objectId;
public string animationName;
public bool waitForCompletion;

// SetAnimatorParameterActionSO
public string objectId;
public string parameterName;
public AnimatorControllerParameterType parameterType;
public float floatValue;
public int intValue;
public bool boolValue;
```

#### **4.2.4 Actions Physique**

```csharp
// ApplyForceActionSO
public string objectId;
public Vector3 force;
public ForceMode forceMode;

// EnableGravityActionSO
public string objectId;
public bool enableGravity;

// FreezeObjectActionSO
public string objectId;
public bool freezePosition;
public bool freezeRotation;
```

### 4.3 Nouvelles Conditions

#### **4.3.1 Conditions Spatiales**

```csharp
// ObjectInZoneConditionSO
public string objectId;
public string zoneId;

// ObjectsProximityConditionSO
public string objectId1;
public string objectId2;
public float maxDistance;

// PlayerLookingAtConditionSO
public string objectId;
public float maxAngle;
public float minDuration;
```

#### **4.3.2 Conditions Temporelles**

```csharp
// TimeOfDayConditionSO
public float minHour;
public float maxHour;

// StepDurationConditionSO
public float minDuration;
public float maxDuration;

// ActionSequenceConditionSO
public List<string> actionSequence;
public float maxTimeBetweenActions;
```

#### **4.3.3 Conditions Complexes**

```csharp
// CompositeConditionSO
public enum LogicOperator { AND, OR, XOR, NOT }
public LogicOperator logicOperator;
public List<StepConditionSO> conditions;

// CounterConditionSO
public string counterId;
public int targetValue;
public ComparisonOperator comparisonOperator;

// InventoryConditionSO
public List<string> requiredItems;
public bool requireAll; // AND vs OR
```

### 4.4 Outils Éditeur Avancés

#### **4.4.1 Mission Flow Visualizer**

**Objectif** : Graphe visuel des étapes et transitions

```csharp
public class MissionFlowWindow : EditorWindow
{
    private MissionConfigSO mission;
    
    void OnGUI()
    {
        // Dessiner les steps comme des nodes
        // Dessiner les connexions basées sur les conditions
        // Permettre le drag & drop pour réorganiser
    }
}
```

**Fonctionnalités** :
- Vue graphique de la mission
- Zoom/Pan
- Création de steps par clic
- Connexions visuelles des transitions

#### **4.4.2 Mission Validator**

**Objectif** : Vérifier la cohérence de la mission

```csharp
public class MissionValidator
{
    public List<ValidationError> Validate(MissionConfigSO mission)
    {
        var errors = new List<ValidationError>();
        
        // Vérifier que les scènes existent
        if (!SceneExists(mission.sceneArt))
            errors.Add(new ValidationError($"Scene '{mission.sceneArt}' not found"));
        
        // Vérifier que tous les objectId sont enregistrés
        foreach (var step in mission.steps)
        {
            foreach (var action in step.onEnterActions)
            {
                if (action is ShowObjectActionSO showAction)
                {
                    if (!ObjectIdExists(showAction.objectId))
                        errors.Add(new ValidationError($"Object '{showAction.objectId}' not registered"));
                }
            }
        }
        
        // Vérifier que toutes les voix off existent
        if (!VoiceOverKeyExists(mission.missionStartVoiceOverKey))
            errors.Add(new ValidationError($"VoiceOver key '{mission.missionStartVoiceOverKey}' not found"));
        
        return errors;
    }
}
```

#### **4.4.3 Mission Tester**

**Objectif** : Tester rapidement une mission

```csharp
public class MissionTesterWindow : EditorWindow
{
    private MissionConfigSO mission;
    private int currentStepIndex;
    
    void OnGUI()
    {
        // Boutons pour naviguer entre les steps
        if (GUILayout.Button("Previous Step"))
            currentStepIndex--;
        
        if (GUILayout.Button("Next Step"))
            currentStepIndex++;
        
        // Bouton pour compléter automatiquement les conditions
        if (GUILayout.Button("Auto-Complete Conditions"))
            AutoCompleteCurrentStep();
        
        // Afficher l'état actuel
        DrawCurrentStepInfo();
    }
    
    void AutoCompleteCurrentStep()
    {
        var step = mission.steps[currentStepIndex];
        foreach (var condition in step.exitConditions)
        {
            if (condition is UserActionDoneConditionSO uaCondition)
            {
                UserActionsManager.MarkActionDone(uaCondition.userActionId);
            }
        }
    }
}
```

#### **4.4.4 Mission Analytics**

**Objectif** : Statistiques sur les missions

```csharp
public class MissionAnalytics
{
    public struct MissionStats
    {
        public int totalSteps;
        public int totalActions;
        public int totalConditions;
        public float estimatedDuration;
        public List<string> usedObjectIds;
        public List<string> usedUserActions;
    }
    
    public MissionStats AnalyzeMission(MissionConfigSO mission)
    {
        var stats = new MissionStats();
        stats.totalSteps = mission.steps.Count;
        
        foreach (var step in mission.steps)
        {
            stats.totalActions += step.onEnterActions.Count + step.onExitActions.Count;
            stats.totalConditions += step.exitConditions.Count;
            
            // Estimer la durée basée sur les DelayActionSO
            foreach (var action in step.onEnterActions)
            {
                if (action is DelayActionSO delay)
                    stats.estimatedDuration += delay.delaySeconds;
            }
        }
        
        return stats;
    }
}
```

### 4.5 Intégrations Externes

#### **4.5.1 Analytics / Telemetry**

```csharp
public class MissionTelemetry
{
    public void TrackMissionStart(string missionId)
    {
        Analytics.CustomEvent("mission_start", new Dictionary<string, object>
        {
            { "mission_id", missionId },
            { "timestamp", DateTime.Now }
        });
    }
    
    public void TrackStepComplete(string stepId, float duration)
    {
        Analytics.CustomEvent("step_complete", new Dictionary<string, object>
        {
            { "step_id", stepId },
            { "duration", duration },
            { "errors", MissionScoreTracker.errorsCount }
        });
    }
}
```

#### **4.5.2 Multiplayer Support**

```csharp
public class MultiplayerMissionSync
{
    public void SyncStepProgress(int stepIndex)
    {
        // Envoyer à tous les clients
        NetworkManager.SendToAll("MISSION_STEP_SYNC", stepIndex);
    }
    
    public void OnUserActionCompleted(string actionId, int playerId)
    {
        // Notifier les autres joueurs
        NetworkManager.SendToAll("USER_ACTION_DONE", new { actionId, playerId });
    }
}
```

#### **4.5.3 Cloud Save**

```csharp
public class CloudMissionSave
{
    public async Task SaveToCloud(MissionSaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData);
        await CloudSaveService.SaveAsync($"mission_{saveData.missionId}", json);
    }
    
    public async Task<MissionSaveData> LoadFromCloud(string missionId)
    {
        string json = await CloudSaveService.LoadAsync($"mission_{missionId}");
        return JsonUtility.FromJson<MissionSaveData>(json);
    }
}
```

### 4.6 Accessibilité

#### **4.6.1 Subtitles System**

```csharp
public class SubtitleActionSO : StepActionSO
{
    public string subtitleKey;
    public float displayDuration;
    
    public override void ExecuteAsync(IMissionContext context, Action onComplete)
    {
        SubtitleManager.ShowSubtitle(subtitleKey, displayDuration);
        onComplete?.Invoke();
    }
}
```

#### **4.6.2 Color Blind Mode**

```csharp
public class ColorBlindHighlightActionSO : StepActionSO
{
    public string objectId;
    public ColorBlindPattern pattern; // Stripes, Dots, etc.
}
```

#### **4.6.3 Difficulty Settings**

```csharp
public enum DifficultyLevel { Easy, Normal, Hard }

public class DifficultyConditionSO : StepConditionSO
{
    public DifficultyLevel requiredDifficulty;
    
    public override bool Evaluate(IMissionContext context)
    {
        return GameSettings.CurrentDifficulty == requiredDifficulty;
    }
}
```

---

## CONCLUSION

Le système de missions est **complet, extensible et production-ready**. Il respecte les principes SOLID, offre une grande flexibilité via les ScriptableObjects, et s'intègre parfaitement avec les systèmes VORTA/EVAVEO existants.

**Points Forts** :
- ✅ Architecture modulaire et maintenable
- ✅ Outils éditeur puissants
- ✅ Support sync/async natif
- ✅ Extensibilité sans modification du core
- ✅ Logs détaillés pour debugging
- ✅ Intégration VoixOffManager multilingue

**Prochaines Étapes Recommandées** :
1. Implémenter le système de sauvegarde (4.1.1)
2. Ajouter le Mission Flow Visualizer (4.4.1)
3. Créer des actions/conditions spécifiques au projet
4. Mettre en place les analytics (4.5.1)

---

**Auteur** : Système de Missions ADIV_SST  
**Date** : Mars 2026  
**Version** : 1.0
