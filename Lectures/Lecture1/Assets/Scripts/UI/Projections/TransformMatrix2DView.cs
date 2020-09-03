using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Projections
{
    public class TransformMatrix2DView: MonoBehaviour
    {
        public TMP_InputField[] Inputs;
        public float2x2 Matrix = float2x2.identity;

        public UnityEvent OnUpdate;

        public void Awake()
        {
            
            for (int id = 0, j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++, id++)
                {
                    // Get the closure to capture i, j by value 
                    int2 coords = new int2(i, j);

                    Inputs[id].text = Matrix[coords.x][coords.y].ToString();

                    Inputs[id].onValueChanged.AddListener(
                        str => {
                            if (float.TryParse(str, out float value))
                            {
                                Matrix[coords.x][coords.y] = value;
                            }

                            OnUpdate.Invoke();
                        }
                    );
                }
            }
        }

        public void OnValidate()
        {
            if (Inputs.Length != 4)
            {
                Inputs = new TMP_InputField[4];
            }

            for (int id = 0, j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++, id++)
                {
                    // Get the closure to capture i, j by value 
                    int2 coords = new int2(i, j);

                    Inputs[id].text = Matrix[coords.x][coords.y].ToString();
                }
            }
        }
    }
}