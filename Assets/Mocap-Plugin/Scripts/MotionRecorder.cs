using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MotionRecorder : MonoBehaviour
{
    public bool isBVHRecorder = false;
    public bool isRecording = false;
    private string msg = "";
    public Avatar avatar;
    private Animator avatarAnimator;
    [SerializeField] private MotionDataRecorder motionDataRecorder;
    [SerializeField] private BVHRecorder bvhRecorder;
    private string savedFolderPath;

    private void Awake()
    {
        motionDataRecorder = GetComponent<MotionDataRecorder>();
        if (avatar != null)
        {
          avatarAnimator = avatar.gameObject.GetComponent<Animator>();
        }
    }

    private void Start()
{
  savedFolderPath =   CheckDirectory(Application.dataPath + @"/Resources");
  if (string.IsNullOrEmpty(savedFolderPath))
    {
      SafeCreateDirectory(Application.dataPath + @"/Resources");
    }
}
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RecordMotion();
        }
    }

    public void RecordMotion()
    {
        msg = "";
        if (!isBVHRecorder)
        {
            if (!isRecording)
            {
                if (motionDataRecorder == null) motionDataRecorder = gameObject.AddComponent<MotionDataRecorder>();
                motionDataRecorder.avatar = avatar;
                motionDataRecorder._animator = avatarAnimator;
                isRecording = true;
                motionDataRecorder.RecordStart();
            }
            else
            {
                try
                {
                    isRecording =false;
                    motionDataRecorder.RecordEnd();
                    msg = "Record Anim Success!";
                }
                catch (System.Exception e)
                {
                    msg = "Record Anim Fail！!";
                    Debug.LogError("Fail！" + e.Message + e.StackTrace);
                }
            }
        }
        else
        {
            if (!isRecording)
            {
                if (bvhRecorder == null) bvhRecorder = gameObject.AddComponent<BVHRecorder>();
                isRecording = true;
                bvhRecorder.targetAvatar = avatarAnimator;
                bvhRecorder.scripted = true;
                bvhRecorder.blender = true;
                bvhRecorder.enforceHumanoidBones = true;
                bvhRecorder.getBones();
                bvhRecorder.rootBone = avatar.jointPoints[PositionIndex.Hip.Int()].transform;
                bvhRecorder.buildSkeleton();
                bvhRecorder.genHierarchy();
                bvhRecorder.capturing = true;
                bvhRecorder.frameRate = 60f;
                bvhRecorder.catchUp = true;
            }
            else
            {
                try
                {
                    isRecording = false;
                    bvhRecorder.capturing = false;
                    var pathName = string.Format("Assets/Resources");
                    var path = Directory.Exists(pathName) ? GetDirectoryPath(pathName) : null;
                    if (path.Length != 0)
                    {
                        FileInfo fi = new FileInfo(path);
                        bvhRecorder.directory = fi.DirectoryName;
                        bvhRecorder.filename = string.Format("RecordMotion_{0}{1:yyyy_MM_dd_HH_mm_ss}.bvh", "motion", DateTime.Now);
                        bvhRecorder.saveBVH();
                    }
                    bvhRecorder.clearCapture();
                    //bvhRecorder = null;
                    msg = "Record BVH Success!";
                }
                catch (System.Exception e)
                {
                    msg = "Record BVH Fail！!";
                }
            }
        }
        Debug.Log(msg);
    }

        public string CheckDirectory(string path)
        {
            return Directory.Exists(path) ? GetDirectoryPath(path) : null;
        }

            public  DirectoryInfo SafeCreateDirectory(string path)
            {
                return Directory.Exists(path) ? null : Directory.CreateDirectory(path);
            }

        private  string GetDirectoryPath(string directory)
        {
            var directoryPath = Path.GetFullPath(directory);
            if (!directoryPath.EndsWith("\\"))
            {
                directoryPath += "\\";
            }
            if (Path.GetPathRoot(directoryPath) == directoryPath)
            {
                return directory;
            }
            return Path.GetDirectoryName(directoryPath) + Path.DirectorySeparatorChar;
        }
}
