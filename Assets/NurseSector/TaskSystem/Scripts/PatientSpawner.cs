using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PatientSpawner : MonoBehaviour
{
    [Tooltip("The patients to spawn. Including limits per task type and custom prefab option.")]
    public PatientToSpawn[] patientsToSpawn;
    [System.Serializable]
    public class PatientToSpawn
    {
        [Header("Initial Poses")]
        [Tooltip("The initial pose for the patient base. If no poses are set, base pose will be set to defaultSleep.")]
        public initialPose BasePose;
        [Tooltip("The initial pose for the patient legs. If the leg pose is set to none and motion is paused, the base pose will be used for that layer.")]
        public initialPose LegPose;
        [Tooltip("When checked, the leg motion will be paused.")]
        public bool pauseLegMotion;
        [Tooltip("The initial pose for the patient full upper body. The upper body includes the torso, arms, and head.")]
        public initialPose FullUpperBodyPose;
        [Tooltip("The initial pose for the patient torso. (will overwrite FullUpperBodyPose)")]
        public initialPose TorsoPose;
        [Tooltip("The initial pose for the patient arms. (will overwrite FullUpperBodyPose)")]
        public initialPose ArmPose;
        [Tooltip("The initial pose for the patients hands. (will overwrite ArmPose, FullUpperBodyPose)")]
        public initialPose HandPose;
        [Tooltip("The initial pose for the patient head. (will overwrite FullUpperBodyPose)")]
        public initialPose HeadPose;

        public enum initialPose
        {
            none,
            defaultSleep,
            eating,
            sleeping,
            wounded1,
            wounded2,
            kneeUpLaying,
            layingLooking,
            layingMildCough,
            layingSleeping,
            pickingAtShirt,
            kidLegSwing,
            kidRubArm,
            parentSitDisbelief,
            parentSitTalk,
            legsCloserTogether,
        }

        [Header("Modular Outfit Choices")]
        [Tooltip("Pants choice for the patient. 0 for none. -1 for random.")]
        public int pantsChoice = 0;
        [Tooltip("Shirt choice for the patient. 0 for none. -1 for random.")]
        public int shirtChoice = 0;
        [Tooltip("Shoes choice for the patient. 0 for none. -1 for random.")]
        public int shoesChoice = 0;
        [Tooltip("Outfit choice for the patient. Outfit overrides all previous choices. 0 for none. -1 for random.")]
        public int outfitChoice = 0;
    }

    [HideInInspector]
    public List<Patient> patients = new List<Patient>();

    public void posePatientParts(Patient patient, PatientToSpawn spawnPoses)
    {
        var m_animator = patient.GetComponentInChildren<Animator>();
        var basePoseNum = (int)spawnPoses.BasePose;
        if(basePoseNum == 0 &&
            (int)spawnPoses.LegPose == 0 &&
            (int)spawnPoses.FullUpperBodyPose == 0 &&
            (int)spawnPoses.TorsoPose == 0 &&
            (int)spawnPoses.ArmPose == 0 &&
            (int)spawnPoses.HandPose == 0 &&
            (int)spawnPoses.HeadPose == 0)
        {
            Debug.Log("Patient " + patient.PatientNumber + ": " + patient.patientName + " - has no pose set. Setting to defaultSleep");
            basePoseNum = 1;
        }

        Debug.Log("Pose " + patient.patientName + ": " + (int)spawnPoses.BasePose + " | legPose: " + (int)spawnPoses.LegPose + " | FullUpperBodyPose: " + (int)spawnPoses.FullUpperBodyPose + " | TorsoPose: " + (int)spawnPoses.TorsoPose + " | ArmPose: " + (int)spawnPoses.ArmPose + " | HeadPose: " + (int)spawnPoses.HeadPose);
        if(m_animator == null)
        {
            return;
        }

        m_animator.SetInteger("basePose", basePoseNum);

        var legPoseNum = (int)spawnPoses.LegPose;

        if(spawnPoses.pauseLegMotion)
        {
            m_animator.SetFloat("legMotionSpeed", 0);
            if(legPoseNum == 0)
            {
                legPoseNum = basePoseNum;
            }
        }
        else
        {
            m_animator.SetFloat("legMotionSpeed", 1);
        }

        m_animator.SetInteger("legPose", legPoseNum);
        m_animator.SetInteger("upperPose", (int)spawnPoses.FullUpperBodyPose);
        m_animator.SetInteger("torsoPose", (int)spawnPoses.TorsoPose);
        m_animator.SetInteger("armPose", (int)spawnPoses.ArmPose);
        m_animator.SetInteger("handPose", (int)spawnPoses.HandPose);
        m_animator.SetInteger("headPose", (int)spawnPoses.HeadPose);
    }

    public void clothePatient(Patient patient, PatientToSpawn spawnClothing)
    {
        clothePatientPart(patient.modularPants, spawnClothing.pantsChoice);
        clothePatientPart(patient.modularShirts, spawnClothing.shirtChoice);
        clothePatientPart(patient.modularShoes, spawnClothing.shoesChoice);
        clothePatientPart(patient.modularOutfits, spawnClothing.outfitChoice);

        void clothePatientPart(GameObject[] modularClothing, int choice)
        {
            if(modularClothing.Length == 0)
            {
                return;
            }

            if(choice == -1){
                choice = Random.Range(1, modularClothing.Length + 1);
            }

            if(choice == 0)
            {
                return;
            }

            modularClothing[choice - 1].SetActive(true);
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if(gameObject.GetComponent<SceneTaskManager>() == null)
        {
            return;
        }

        if(patientsToSpawn.Length < gameObject.GetComponent<SceneTaskManager>().taskHostsToSpawn.Length)
        {
            var tempList = patientsToSpawn.ToList();
            for(int i = patientsToSpawn.Length; i < gameObject.GetComponent<SceneTaskManager>().taskHostsToSpawn.Length; i++)
            {
                tempList.Add(new PatientToSpawn());
            }
            patientsToSpawn = tempList.ToArray();
        }
        else if(patientsToSpawn.Length > gameObject.GetComponent<SceneTaskManager>().taskHostsToSpawn.Length)
        {
            var tempList = patientsToSpawn.ToList();
            for(int i = patientsToSpawn.Length - 1; i >= gameObject.GetComponent<SceneTaskManager>().taskHostsToSpawn.Length; i--)
            {
                tempList.RemoveAt(i);
            }
            patientsToSpawn = tempList.ToArray();
        }

        foreach(PatientToSpawn patient in patientsToSpawn)
        {
            Patient myPatient = null;
            if(myPatient == null)
            {
                myPatient = gameObject.GetComponent<SceneTaskManager>().taskHostPrefab.GetComponent<Patient>();
            }
            else
            {
                myPatient = gameObject.GetComponent<SceneTaskManager>().taskHostsToSpawn[System.Array.IndexOf(patientsToSpawn, patient)].CustomTaskHostPrefab.GetComponent<Patient>();
            }

            if(myPatient == null)
            {
                Debug.LogError("Patient prefab does not have a Patient component.");
                return;
            }

            patient.pantsChoice = Mathf.Clamp(patient.pantsChoice, -1, myPatient.modularPants.Length);
            patient.shirtChoice = Mathf.Clamp(patient.shirtChoice, -1, myPatient.modularShirts.Length);
            patient.shoesChoice = Mathf.Clamp(patient.shoesChoice, -1, myPatient.modularShoes.Length);
            patient.outfitChoice = Mathf.Clamp(patient.outfitChoice, -1, myPatient.modularOutfits.Length);

            if(patient.outfitChoice != 0){
                patient.shirtChoice = 0;
                patient.shoesChoice = 0;
                patient.pantsChoice = 0;
            }
        }
    }
    
}