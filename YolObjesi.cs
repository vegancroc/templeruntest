using UnityEngine;

public enum YolTuru { DuzYol = 0, SagDonemec = 1, SolDonemec = 2, SagVeSolDonemec = 3 };

public class YolObjesi : MonoBehaviour
{
	public YolTuru yolTuru;
	public int prefabTuru;

	public Vector3 ebatlar;

	public Transform solPuanSpawnNoktalari;
	public Transform sagPuanSpawnNoktalari;
}