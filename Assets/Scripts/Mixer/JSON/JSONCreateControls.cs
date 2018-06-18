using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSONCreateControls {
	[SerializeField]
	string type = "method";
	[SerializeField]
	int id = 123;
	[SerializeField]
	string method;

	[SerializeField]
	ControlsParams @params;

	public JSONCreateControls(GameManager.MethodType methodType, ControlsParams parameters) {
		method = methodType.ToString();
		@params = parameters;
	}


	public string SaveToString() {
		return JsonUtility.ToJson(this);
	}
}
