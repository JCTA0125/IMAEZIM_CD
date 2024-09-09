using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Google.XR.ARCoreExtensions.Samples.Geospatial;

public class LoadGallery : MonoBehaviour
{
    public RawImage img;
    public GeospatialController controllerScript;
    //public GameObject cube;


    public void OnClickImageLoad()
    {
        NativeGallery.GetImageFromGallery((file) =>
        {

            FileInfo selected = new FileInfo(file);


            if (selected.Length > 50000000)
            {
                return;
            }

            byte[] fileData = File.ReadAllBytes(file);

            string base64String = Convert.ToBase64String(fileData); 

            PlayerPrefs.SetString("MemoPicture", base64String);
            
            var temp = File.ReadAllBytes(file); 
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(temp); 
            img.texture = tex;
            
        });

        controllerScript.OnApplicationFocus();

    }

    
    IEnumerator LoadImage(string path)
    {
        yield return null;

    }

    
}
