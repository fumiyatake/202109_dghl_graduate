using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.VFX.Utility
{

    public class SpawnCuve : VFXOutputEventAbstractHandler
    {

        public override bool canExecuteInEditor => true;

        [SerializeField] GameObject cube;

        static readonly int kPosition = Shader.PropertyToID("position");

        public override void OnVFXOutputEvent(VFXEventAttribute eventAttribute)
        {

            Vector3 position = eventAttribute.GetVector3(kPosition);
            position.y += 8f;
            Debug.Log( position );
//            Instantiate(cube, position, cube.transform.rotation);
        }
    }
}