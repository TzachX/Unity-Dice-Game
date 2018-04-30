using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFloor : MonoBehaviour {

    public static List<string> cubes;

	// Use this for initialization
	void Start () {
        ResetCubes();
    }
	
	// Update is called once per frame
	void Update () {
        //print(cubes.Count);
	}

    private void OnTriggerStay(Collider other)
    {
        if(other.attachedRigidbody.velocity.magnitude < 0.5f)
        {
            if (!cubes.Contains(other.attachedRigidbody.ToString()))
            {
                other.attachedRigidbody.gameObject.GetComponent<Dice>().HasLanded = true;
                switch (other.name)
                {
                    case "SwordsI":
                        other.attachedRigidbody.gameObject.GetComponent<Dice>().ValueAfterThrow = (int)cubeValues.Skull;
                        break;
                    case "SwordsII":
                        other.attachedRigidbody.gameObject.GetComponent<Dice>().ValueAfterThrow = (int)cubeValues.Shield;
                        break;
                    case "Skull":
                        other.attachedRigidbody.gameObject.GetComponent<Dice>().ValueAfterThrow = (int)cubeValues.Swords;
                        break;
                    case "Reroll":
                        other.attachedRigidbody.gameObject.GetComponent<Dice>().ValueAfterThrow = (int)cubeValues.Reroll;
                        break;
                    case "Shield":
                        other.attachedRigidbody.gameObject.GetComponent<Dice>().ValueAfterThrow = (int)cubeValues.Swords;
                        break;
                }
                cubes.Add(other.attachedRigidbody.ToString());
            }
        }
    }

    public static void ResetCubes()
    {
        cubes = new List<string>();
    }
}
