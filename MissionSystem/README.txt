MISSION SYSTEM - GUIDE D'UTILISATION
=====================================

ARCHITECTURE COMPLETE IMPLEMENTEE
----------------------------------

Le système de missions est maintenant complètement implémenté avec:
- Architecture SOLID respectée
- Séparation stricte Système/Data
- Support des callbacks et actions asynchrones
- Custom Inspectors avec boutons helper
- Intégration complète avec VORTA/EVAVEO

DEMARRAGE RAPIDE
----------------

1. CREER UNE MISSION:
   - Menu: GAME > Mission System > Mission Manager Window
   - Entrer le nom de la mission
   - Cliquer "Create Mission with Folder Structure"
   - La mission est créée avec son dossier

2. CONFIGURER LA MISSION:
   - Sélectionner le MissionConfigSO créé
   - Remplir: scènes (Art + Interaction), position spawn, mode
   - Cliquer "Create New Step" pour ajouter des étapes

3. CONFIGURER UNE ETAPE:
   - Sélectionner le MissionStepConfigSO
   - Cliquer "Add Action" pour ajouter des actions (VoiceOver, ShowObject, etc.)
   - Cliquer "Add Condition" pour ajouter des conditions de sortie
   - Les assets sont créés automatiquement dans le bon dossier

4. PREPARER LA SCENE:
   - Menu: GameObject > GAME > Mission System > Mission Manager
   - Cela crée le MissionManager avec tous les services
   - Sur les objets importants: GameObject > GAME > Mission System > Mission Object Registrar
   - Configurer les objectId

5. LANCER:
   - Play Mode
   - Appeler MissionManager.Instance.StartMission(missionConfig)

ACTIONS DISPONIBLES
-------------------

SYNCHRONES (exécution immédiate):
- ShowObjectActionSO - Affiche un objet
- HideObjectActionSO - Cache un objet
- ShowGhostActionSO - Affiche un ghost
- HideGhostActionSO - Cache un ghost
- EnableUserActionActionSO - Active une UserAction
- DisableUserActionActionSO - Désactive une UserAction
- TeleportPlayerActionSO - Téléporte le joueur

ASYNCHRONES (avec callback):
- PlayVoiceOverActionSO - Joue une voix off via VoixOffManager (utilise une clé string)
  * Attend la fin si waitForCompletion=true
  * Intégré avec PLS_VoiceOverData pour la gestion multilingue
- WaitForObjectPickedActionSO - Attend que le joueur sélectionne un objet
- WaitForUserActionActionSO - Attend qu'une UserAction soit faite
- DelayActionSO - Attend X secondes
- SequenceActionSO - Exécute plusieurs actions en séquence

CONDITIONS DISPONIBLES
----------------------

- UserActionDoneConditionSO - Vérifie si une UserAction est faite
- AllUserActionsDoneConditionSO - Vérifie si toutes les UserActions sont faites
- MissionModeConditionSO - Vérifie le mode (Formation/Évaluation)
- TimerConditionSO - Vérifie si X secondes se sont écoulées
- ObjectActiveConditionSO - Vérifie si un objet est actif/inactif

STRUCTURE DES FICHIERS
----------------------

Assets/GAME/Scripts/MissionSystem/
├── Core/                          (Système principal)
│   ├── IMissionContext.cs
│   ├── IMissionState.cs
│   ├── MissionManager.cs
│   ├── MissionStateMachine.cs
│   └── MissionStepState.cs
│
├── Data/                          (ScriptableObjects)
│   ├── MissionConfigSO.cs
│   ├── MissionStepConfigSO.cs
│   ├── StepActionSO.cs
│   └── StepConditionSO.cs
│
├── Services/                      (Services injectés)
│   ├── Interfaces/
│   └── Implementations/
│
├── Actions/                       (Actions extensibles)
│   ├── Sync/
│   └── Async/
│
├── Conditions/                    (Conditions extensibles)
│
├── Helpers/                       (Utilitaires)
│   ├── IPickable.cs
│   ├── PickableObject.cs
│   └── MissionObjectRegistrar.cs
│
└── Editor/                        (Outils éditeur)
    ├── MissionConfigSOEditor.cs
    ├── MissionStepConfigSOEditor.cs
    ├── MissionSystemEditorWindow.cs
    └── MissionSystemMenuItems.cs

EXEMPLE DE WORKFLOW
-------------------

1. Créer mission "Mission_Boucherie"
   - missionStartVoiceOverKey: "Mission_Boucherie_Intro"
   - missionCompleteVoiceOverKey: "Mission_Boucherie_Complete"

2. Créer Step_01 "Introduction"
   - stepVoiceOverKey: "Mission_Boucherie_Step01"
   - Action: PlayVoiceOver (voiceOverKey: "Boucherie_Bienvenue", waitForCompletion: true)
   - Action: ShowObject ("Couteau")
   - Action: WaitForObjectPicked ("Couteau")
   - Action: PlayVoiceOver (voiceOverKey: "Boucherie_TresBien", waitForCompletion: true)
   - Condition: UserActionDone("PRENDRE_COUTEAU")

3. Créer Step_02 "Découpe"
   - stepVoiceOverKey: "Mission_Boucherie_Step02"
   - Action: ShowObject ("Planche")
   - Action: EnableUserAction("COUPER_VIANDE")
   - Condition: UserActionDone("COUPER_VIANDE")

NOTE: Les clés doivent correspondre aux entrées dans PLS_VoiceOverData

4. Dans la scène:
   - Ajouter MissionManager
   - Sur le couteau: MissionObjectRegistrar (id="Couteau") + PickableObject
   - Sur la planche: MissionObjectRegistrar (id="Planche")

5. Tester en Play Mode

EXTENSIBILITE
-------------

Pour créer une nouvelle action:
1. Créer un fichier héritant de StepActionSO
2. Implémenter ExecuteAsync(context, onComplete)
3. Ajouter [CreateAssetMenu]
4. L'action apparaît automatiquement dans les menus

Pour créer une nouvelle condition:
1. Créer un fichier héritant de StepConditionSO
2. Implémenter Evaluate(context)
3. Ajouter [CreateAssetMenu]
4. La condition apparaît automatiquement dans les menus

INTEGRATION VORTA/EVAVEO
------------------------

Le système s'intègre avec:
- UserActionsManager (gestion des actions utilisateur)
- ScenesManager (chargement de scènes avec écran de chargement)
- GhostTraceBase (affichage/masquage de ghosts)
- Grabable (objets saisissables)
- EvaveoMenuVR (menu de sélection de missions)
- VoixOffManager (système de voix off avec clés et support multilingue)

INTEGRATION VOIXOFFMANAGER
--------------------------

Le système de missions utilise VoixOffManager pour la lecture des voix off:

1. CONFIGURATION:
   - Les voix off sont référencées par des CLÉS STRING (ex: "Mission_Intro", "Step_01_VO")
   - Les clips audio sont stockés dans PLS_VoiceOverData (ScriptableObject)
   - Support automatique du multilingue (_fr, _eng, etc.)

2. UTILISATION DANS LES MISSIONS:
   - MissionConfigSO:
     * missionStartVoiceOverKey: Clé de la VO de démarrage
     * missionCompleteVoiceOverKey: Clé de la VO de fin
   
   - MissionStepConfigSO:
     * stepVoiceOverKey: Clé de la VO de l'étape
   
   - PlayVoiceOverActionSO:
     * voiceOverKey: Clé de la VO à jouer
     * waitForCompletion: Attend la fin du clip

3. AVANTAGES:
   - Gestion centralisée des voix off
   - Changement de langue automatique
   - Pas de références directes aux AudioClip
   - Synchronisation avec Google Drive possible
   - Fallback automatique si clip manquant

SUPPORT
-------

Tous les composants ont des logs détaillés préfixés par [MissionSystem]
Activer la console pour suivre l'exécution des missions.

Pour toute question, consulter les commentaires dans le code source.
