using UnityEngine;

// Karakteri (Player) hareket ettirirken kullandığımız
// inputları düzenli bir şekilde depolayan, kontrol eden script
// Script hem mobil hem de PC platformlarını desteklemektedir.
// (Player, hareket ederken gerekli inputları bu scriptten çekiyor)
public class InputManager : MonoBehaviour
{
	// bu script'e her yerden erişmek için property
	public static InputManager Instance { get; private set; }

	// telefonun sensörün hassaslığı
	public float mobilSensorSensitivity = 3.5f;
	// dokunmatik ekranda parmağı kaç pixel sürükleyince
	// o finger swipe'ın bir input olarak varsayılacağını
	// depolayan değişken
	public float mobilTouchSensitivity = 30f;

#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
	/*
	** Bu kod parçası sadece mobil cihazlarda çalışır
	*/
	private int parmakId = -1;
	private Vector2 parmakIlkPosition;
	private float sensorOutput = 0f;
#endif

	private void Awake()
	{
		if( Instance == null )
			Instance = this;
		else if( this != Instance )
		{
			Destroy( this );
			return;
		}

#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
		/*
		** Bu kod parçası sadece mobil cihazlarda çalışır
		*/
		// Update'te sqrMagnitude kullandığımızdan dolayı bu değişkenin karesini alıyoruz
		mobilTouchSensitivity *= mobilTouchSensitivity;
#endif
	}

	private void Update()
	{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
		/*
		** Bu kod parçası sadece Unity editöründe, PC, MAC, Linux ve Web Player'da çalışır
		*/
		// Eğer space tuşuna veya farenin sol tuşuna basılırsa player'ı zıplat
		if( Input.GetKeyDown( KeyCode.Space ) || Input.GetMouseButtonDown( 0 ) )
			Player.Instance.Zipla();

		// Eğer A tuşuna basılırsa karakteri sol yol ayrımına saptır
		if( Input.GetKeyDown( KeyCode.A ) )
			Player.Instance.SolaDon();

		// Eğer D tuşuna basılırsa karakteri sağ yol ayrımına saptır
		if( Input.GetKeyDown( KeyCode.D ) )
			Player.Instance.SagaDon();
#else
		/*
		** Bu kod parçası sadece mobil cihazlarda çalışır
		*/
		// Sensör outputuna low-pass filter uyguluyoruz
		// (sensörün değerinin nispeten "yumuşak" bir şekilde değişmesi için)
		float hedefSensorDegeri = ( Input.acceleration.x ) * mobilSensorSensitivity;
		sensorOutput = Mathf.Lerp( sensorOutput, hedefSensorDegeri, 0.25f );
		
		// Multi-touch desteği için ekrandaki tüm parmakların (Input.touches) üzerinden 
		// tek tek geçiyoruz
		for( int i = 0; i < Input.touchCount; i++ )
		{
			Touch parmak = Input.GetTouch( i );

			if( parmakId == -1 && parmak.phase == TouchPhase.Began )
			{
				// Eğer parmakId -1 ise (yani ekranda hareketini takip etmekte olduğumuz bir parmak yoksa)
				// ve üzerinde bulunduğumuz parmak ekrana yeni dokunmuşsa:
				// üzerinde bulunduğumuz parmağı takibe al
				parmakId = parmak.fingerId;
				parmakIlkPosition = parmak.position;
			}
			else if( parmak.fingerId == parmakId )
			{
				// Eğer ki üzerinde bulunduğumuz parmak, hareketini takip etmekte olduğumuz parmak ise:
				if( parmak.phase != TouchPhase.Ended && parmak.phase != TouchPhase.Canceled )
				{
					// Eğer parmağın ekranla bağlantısı kesilmemişse (parmak ekrandan kaldırılmamışsa):
					// parmağın son konumuyla ilk konumu arasındaki Vector2 farkını bul
					Vector2 deltaPosition = parmak.position - parmakIlkPosition;
					if( deltaPosition.sqrMagnitude >= mobilTouchSensitivity )
					{
						// Eğer deltaPosition'ın büyüklüğünün karesi (Vector'lerin büyüklüğünün karesini bulmak,
						// kendisini bulmaktan daha kolay ve hızlıdır) sensitivity'den büyükse (bu yüzden Start
						// fonksiyonunda sensitivity'nin karesini aldık; iki uzunluğun karelerini kıyaslıyoruz):
						float x = deltaPosition.x;
						float y = deltaPosition.y;
						
						if( y > Mathf.Abs( x ) )
						{
							// eğer parmak y ekseninde (yukarı yönde) daha çok hareket etmişse player'ı zıplat
							Player.Instance.Zipla();
						}
						else if( x > 0f )
						{
							// eğer parmak sağ yönde daha çok hareket etmişse player'ı sağ yol ayrımına saptır
							Player.Instance.SagaDon();
						}
						else
						{
							// eğer parmak sol yönde daha çok hareket etmişse player'ı sol yol ayrımına saptır
							Player.Instance.SolaDon();
						}
						
						// bu parmakla işimiz bitti, artık herhangi bir parmağın hareketini takip etmediğimizi
						// sisteme bildir
						parmakId = -1;
					}
				}
				else
				{
					// Eğer takip ettiğimiz parmak ekrandan kaldırılmışsa parmakId'yi -1 yaparak
					// artık herhangi bir parmağı takip etmediğimizi belirle
					parmakId = -1;
				}
			}
		}
#endif
	}

	// Karakteri yatay eksende hareket ettirirken bu hareketin miktarını 
	// belirlemeye yarayan input'u alırken kullandığımız fonksiyon
	public float YatayInputAl()
	{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
		/*
		** Bu kod parçası sadece Unity editöründe, PC, MAC, Linux ve Web Player'da çalışır
		*/
		// mouse'nin yatayda ne kadar hareket ettiğini bul
		return Input.GetAxis( "Mouse X" );
#else
		/*
		** Bu kod parçası sadece mobil cihazlarda çalışır
		*/
		// telefon sensörünün değerinin -1 ile 1 arasında olmasını sağla ve bu değeri döndür
		return Mathf.Clamp( sensorOutput, -1f, 1f );
#endif
	}
}