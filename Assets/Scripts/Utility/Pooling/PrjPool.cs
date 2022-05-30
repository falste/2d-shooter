using UnityEngine;
using System.Collections.Generic;

public class PrjPool : MonoPool<Projectile>{
	public static PrjPool Instance { get; private set; }

	void OnValidate() {
		Instance = this;	
	}
}
