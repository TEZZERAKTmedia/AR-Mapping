// This file is a duplicate of ARInteractorSpawnTrigger.cs
// but modified to use LimitedObjectSpawner instead of ObjectSpawner

#if AR_FOUNDATION_PRESENT
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets
{
    public class LimitedARInteractorSpawnTrigger : MonoBehaviour
    {
        public enum SpawnTriggerType
        {
            SelectAttempt,
            InputAction,
        }

        [SerializeField]
        XRRayInteractor m_ARInteractor;
        public XRRayInteractor arInteractor
        {
            get => m_ARInteractor;
            set => m_ARInteractor = value;
        }

        [SerializeField]
        LimitedObjectSpawner m_LimitedObjectSpawner;
        public LimitedObjectSpawner limitedObjectSpawner
        {
            get => m_LimitedObjectSpawner;
            set => m_LimitedObjectSpawner = value;
        }

        [SerializeField]
        bool m_RequireHorizontalUpSurface;
        public bool requireHorizontalUpSurface
        {
            get => m_RequireHorizontalUpSurface;
            set => m_RequireHorizontalUpSurface = value;
        }

        [SerializeField]
        SpawnTriggerType m_SpawnTriggerType;
        public SpawnTriggerType spawnTriggerType
        {
            get => m_SpawnTriggerType;
            set => m_SpawnTriggerType = value;
        }

        [SerializeField]
        XRInputButtonReader m_SpawnObjectInput = new XRInputButtonReader("Spawn Object");
        public XRInputButtonReader spawnObjectInput
        {
            get => m_SpawnObjectInput;
            set => XRInputReaderUtility.SetInputProperty(ref m_SpawnObjectInput, value, this);
        }

        [SerializeField]
        bool m_BlockSpawnWhenInteractorHasSelection = true;
        public bool blockSpawnWhenInteractorHasSelection
        {
            get => m_BlockSpawnWhenInteractorHasSelection;
            set => m_BlockSpawnWhenInteractorHasSelection = value;
        }

        bool m_AttemptSpawn;
        bool m_EverHadSelection;

        void OnEnable()
        {
            m_SpawnObjectInput.EnableDirectActionIfModeUsed();
        }

        void OnDisable()
        {
            m_SpawnObjectInput.DisableDirectActionIfModeUsed();
        }

        void Start()
        {
            if (m_LimitedObjectSpawner == null)
#if UNITY_2023_1_OR_NEWER
                m_LimitedObjectSpawner = FindAnyObjectByType<LimitedObjectSpawner>();
#else
                m_LimitedObjectSpawner = FindObjectOfType<LimitedObjectSpawner>();
#endif

            if (m_ARInteractor == null)
            {
                Debug.LogError("Missing AR Interactor reference, disabling component.", this);
                enabled = false;
            }
        }

        void Update()
        {
            if (m_AttemptSpawn)
            {
                m_AttemptSpawn = false;

                if (m_ARInteractor.hasSelection)
                    return;

                var isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
                if (!isPointerOverUI && m_ARInteractor.TryGetCurrentARRaycastHit(out var arRaycastHit))
                {
                    if (!(arRaycastHit.trackable is ARPlane arPlane))
                        return;

                    if (m_RequireHorizontalUpSurface && arPlane.alignment != PlaneAlignment.HorizontalUp)
                        return;

                    m_LimitedObjectSpawner.TrySpawn("Docking_Station", arRaycastHit.pose.position, Quaternion.LookRotation(arPlane.normal), out _);
                }
                return;
            }

            var selectState = m_ARInteractor.logicalSelectState;

            if (m_BlockSpawnWhenInteractorHasSelection)
            {
                if (selectState.wasPerformedThisFrame)
                    m_EverHadSelection = m_ARInteractor.hasSelection;
                else if (selectState.active)
                    m_EverHadSelection |= m_ARInteractor.hasSelection;
            }

            m_AttemptSpawn = false;
            switch (m_SpawnTriggerType)
            {
                case SpawnTriggerType.SelectAttempt:
                    if (selectState.wasCompletedThisFrame)
                        m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                    break;

                case SpawnTriggerType.InputAction:
                    if (m_SpawnObjectInput.ReadWasPerformedThisFrame())
                        m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                    break;
            }
        }
    }
}
#endif
