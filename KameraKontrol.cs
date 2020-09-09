using UnityEngine;

// Kameranın karakteri (hedef) takip etmesini sağlayan script
public class KameraKontrol : MonoBehaviour
{
	// bu script'e her yerden erişmek için property
	public static KameraKontrol Instance { get; private set; }

	// Takip edilecek obje
	public Transform hedef;

	// Kamerayla hedef arasındaki istenen uzaklık (yükseklik farkını hesaba katmadan)
	public float uzaklik = 6f;
	// Kamerayla hedef arasındanki istenen yükseklik farkı
	public float yukseklik = 5f;

	private void Awake()
	{
		if( Instance == null )
			Instance = this;
		else if( this != Instance )
		{
			Destroy( this );
			return;
		}
	}

	private void FixedUpdate()
	{
		// Kameranın gitmesi gereken pozisyon
		Vector3 hedefKonum = hedef.position - hedef.forward * uzaklik + new Vector3( 0f, yukseklik, 0f );

		// Player sahneden aşağı düşse de kamera belli bir y değerinin altına inmesin
		if( hedefKonum.y < yukseklik )
			hedefKonum.y = yukseklik;

		// Kamerayı hedefKonum'a doğru "yumuşak" bir şekilde hareket ettir
		transform.localPosition = Vector3.Slerp( transform.localPosition, hedefKonum, 0.1f );

		// Kamerayı "yumuşak" bir şekilde hedef'e doğru döndür
		Quaternion rot = Quaternion.LookRotation( hedef.position - transform.localPosition );
		transform.localRotation = Quaternion.Slerp( transform.localRotation, rot, 0.1f );
	}
}