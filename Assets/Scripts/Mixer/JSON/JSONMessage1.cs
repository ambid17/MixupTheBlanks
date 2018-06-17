using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSONMessage1 {
	[SerializeField]
	string type = "method";
	[SerializeField]
	int id = 123;
	[SerializeField]
	string method;

	[SerializeField]
	ControlsParams @params;

	public JSONMessage1(MethodType methodType, ControlsParams parameters) {
		method = methodType.ToString();
		@params = parameters;
	}


	public string SaveToString() {
		return JsonUtility.ToJson(this);
	}

	public enum MethodType {
		createControls, deleteControls, createScenes, deleteScene, onSceneDelete
	}
}
