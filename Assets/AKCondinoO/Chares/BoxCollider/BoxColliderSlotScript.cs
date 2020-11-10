using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMA
{
	public class BoxColliderSlotScript : MonoBehaviour
	{
		public void OnDnaApplied(UMAData umaData)
		{
			var rigid = umaData.gameObject.GetComponent<Rigidbody>();
			if (rigid == null)
			{
				rigid = umaData.gameObject.AddComponent<Rigidbody>();
			}
			rigid.constraints = RigidbodyConstraints.FreezeRotation;
			rigid.mass = umaData.characterMass;

			CapsuleCollider capsule = umaData.gameObject.GetComponent<CapsuleCollider>();
			BoxCollider box = umaData.gameObject.GetComponent<BoxCollider>();

			if(umaData.umaRecipe.raceData.umaTarget == RaceData.UMATarget.Humanoid)
			{
				if (box == null)
				{
					box = umaData.gameObject.AddComponent<BoxCollider>();
				}
				if(capsule != null)
				{
					Destroy(capsule);
				}
				//if (capsule == null)
				//{
				//	capsule = umaData.gameObject.AddComponent<CapsuleCollider>();
				//}
				//if( box != null )
				//{
				//	Destroy(box);
				//}
				
					box.size = new Vector3(
						umaData.characterRadius*1.5f,
						umaData.characterHeight*.95f,
						umaData.characterRadius*1.5f);
					box.center = new Vector3(0, box.size.y / 2, 0);
				//capsule.radius = umaData.characterRadius;
				//capsule.height = umaData.characterHeight;
				//capsule.center = new Vector3(0, capsule.height / 2, 0);
			}
			else
			{
				if (box == null)
				{
					box = umaData.gameObject.AddComponent<BoxCollider>();
				}
				if(capsule != null)
				{
					Destroy(capsule);
				}

				//with skycar this capsule collider makes no sense so we need the bounds to figure out what the size of the box collider should be
				//we will assume that renderer 0 is the base renderer
				var umaRenderer = umaData.GetRenderer(0);
				if (umaRenderer != null)
				{
					box.size = umaRenderer.bounds.size;
					box.center = umaRenderer.bounds.center;
				}
			}
		}
	}
}
