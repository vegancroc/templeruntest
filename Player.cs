using UnityEngine;
using UnityEngine.SceneManagement;

// Player'ı (karakteri) kontrol eden script
public class Player : MonoBehaviour
{
	// bu script'e her yerden erişmek için property
	public static Player Instance { get; private set; }

	public SonsuzYolScript yolGenerator;

	// oyun bitince çalan ses dosyası
	public AudioClip gameOverSesi;
	// puan toplayınca çalan ses dosyası
	public AudioClip puanSesi;

	// Objenin sahip olduğu ve sık sık eriştiğimiz tüm component'leri
	// birer değişkende tutuyoruz, böylece onlara erişmemiz daha
	// hızlı oluyor (performans açısından)
	private Rigidbody rb;
	private Animation anim;
	private AudioSource ses;

	private float skor = 0f;

	// Karakterin koşma hızı
	public float maksimumHiz = 10f;
	// Karakterin sağa-sola hareket etme hızı
	public float yatayHiz = 5f;
	// Karakterin zıplama hızı
	public float ziplamaHizi = 10f;

	// Her 10 saniyede bir karakterin koşma hızı (maksimumHiz)
	// 1 artacak (oyun gittikçe hızlanacak/zorlaşacak)
	public float hizArtmaAraligi = 10f;
	private float maksimumHizArtmaZamani;

	// Boşluktan aşağı düşerken true olan bir değişken
	private bool ucurum = false;
	// Karakter ölünce true olan bir değişken
	private bool death = false;

	// Temple Run'da bir zemin türü hariç diğer zemin türlerinde
	// karakteri yolun kenarlarından düşürmek mümkün değil,
	// telefonu ne kadar sağa yatırsak da bir noktadan sonra karakter
	// sağa hareket etmeyi kesiyor. İşte ben de bu sistemi çeşitli metodlar
	// kullanarak oyuna eklemeye çalıştım ama hep başarısız oldum. Örneğin
	// Rigidbody'e sağ-sol yönde güç uygulamayı denedim ama karakter duvara
	// çarptıkça sürtünmenin etkisiyle yavaşlıyor, bazen de yavaşlamasa bile
	// takılıyor izlenimi uyandırıyordu. En sonunda karar kıldığım yöntem
	// karakterin yatay eksende hareket edebileceği koordinatları elle sınırlamak oldu.
	// Karakter yatay eksende bu minimum ve maksimum limit değerlerin dışına 
	// çıkamıyor. Bu, elbette ki çok güzel bir çözüm yolu değil. Mevcut sistemle
	// kenarlarından düşmenin mümkün olduğu zeminler yapmak mümkün değil (belki
	// bu limit değer aralığını çok artırırsak mümkün olabilir). Daha iyi bir sistem 
	// bulmak/düşünmek ve bu sistemi oyuna entegre etmek size kalmış birşey :D
	[HideInInspector]
	public float limitMinDeger = -2.75f;
	[HideInInspector]
	public float limitMaxDeger = 2.75f;

	// -1 sol
	// 0 hiçbir yön
	// 1 sağ
	// Kavşağa gelince hangi yöne döneceğimizi belirleyen değişken
	private int donusYonu = 0;
	// Kavşağa girip girmediğimizi belirleyen değişken
	private bool donemecteyiz = false;

	// Karakterin yüzünü döndüğü yön ileriYon'de, kendisine göre doğu yönü
	// ise sagYon'de depolamakta. Bu değişkenleri kullanarak karakteri o
	// yönlerde hareket ettiriyoruz.
	[HideInInspector]
	public YolYonu yon;
	[HideInInspector]
	public Vector3 ileriYon, sagYon;

	private void Awake()
	{
		if( Instance == null )
			Instance = this;
		else if( this != Instance )
		{
			Destroy( this );
			return;
		}

		rb = GetComponent<Rigidbody>();
		anim = GetComponentInChildren<Animation>();
		ses = GetComponent<AudioSource>();
	}

	public void HareketeBasla()
	{
		anim.Play( "KosmaAnimasyonu" );

		// koşma hızının artacağı zamanı hesapla
		maksimumHizArtmaZamani = Time.time + hizArtmaAraligi;
	}

	private void FixedUpdate()
	{
		// Karakter (player) hâlâ hayattaysa:
		if( !ucurum && !death )
		{
			Vector3 konum = rb.position;

			// karakteri ileri yönde hareket ettir
			Vector3 surat = ileriYon * maksimumHiz * Time.fixedDeltaTime;

			// karakteri yatay eksende hareket ettir
			// yatay eksende hareket miktarı için gerekli input'u InputManager scriptinden al
			surat += sagYon * InputManager.Instance.YatayInputAl() * yatayHiz * Time.fixedDeltaTime;

			konum += surat;

			// Karakterin yatay eksendeki hareketini minimum ve maksimum limit değerler 
			// aralığıyla sınırla
			if( yon == YolYonu.Ileri || yon == YolYonu.Geri )
				konum.x = Mathf.Clamp( konum.x, limitMinDeger, limitMaxDeger );
			else
				konum.z = Mathf.Clamp( konum.z, limitMinDeger, limitMaxDeger );

			rb.position = konum;

			// Rigidbody'nin y ekseni harici hız (velocity) almasını engelle,
			// bu yönlerde hareketi velocity ile değil rb.position ile veriyoruz
			Vector3 v = rb.velocity;
			v.x = 0f;
			v.z = 0f;
			rb.velocity = v;
		}
	}

	private void Update()
	{
		float yukseklik = transform.localPosition.y;

#if UNITY_EDITOR
		// Eğer oyunu Unity editöründe test ederken R tuşuna basarsak oyuna restart at
		// Bu komut sadece Unity editöründe çalışıyor çünkü oyunun yayımlanmış
		// versiyonunda ulu orta restart atmaya yarayan bir komut istemiyoruz
		if( Input.GetKeyDown( KeyCode.R ) )
			SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
#endif

		// Eğer karakterin dikey eksendeki konumu çok alçaksa (karakter bir
		// boşluktan epeyce aşağıya düşmüşse) oyunu bitir
		if( !death && yukseklik < -5f )
		{
			death = true;
			GameOver();
			Time.timeScale = 0f;
		}

		// Eğer karakter hâlâ hayattaysa yapılacaklar:
		if( !ucurum && !death )
		{
			// Skoru, karakterin koşma hızıyla doğru orantılı bir şekilde artır
			skor += Time.deltaTime * maksimumHiz;
			Arayuz.Instance.OyunIcıSkoruGuncelle( (int) skor );

			// Eğer karakter dikey eksende 1.9f'ten daha alçak bir konumdaysa boşluktan
			// düşmek üzeredir ve bu durumda ucurum değişkeninin değerini true yap.
			// ucurum'un değeri true olduğu zaman karakteri hareket ettirmek
			// mümkün olmuyor ve oyuncu da karakterin düşüşünü üzgün bir ifadeyle
			// izlemek zorunda kalıyor (ta ki karakterin y'si -5'e ulaşıp da oyun bitene kadar)
			if( yukseklik < 1.9f )
			{
				ucurum = true;

				// karakter boşluktan düşeceği vakit collider'ını kapatıyoruz çünkü bazen garip
				// bir şekilde karakterin collider'ı bir zeminle temas halinde kalıyor ve karakter
				// hiç hareket etmiyor, sonsuza dek öyle kalıyor
				GetComponentInChildren<Collider>().enabled = false;

				// Skoru saydamlaştır, karakter yere düşerken skor zeminin içine girince çirkin duruyor
				Arayuz.Instance.StartCoroutine( Arayuz.Instance.SkorYazisiniSaydamYap() );
			}

			// eğer karakterin koşma hızını artırmanın vakti geldiyse koşma hızını artır ve
			// hızın artırılacağı bir sonraki anı hesapla
			if( Time.time >= maksimumHizArtmaZamani )
			{
				maksimumHizArtmaZamani = Time.time + hizArtmaAraligi;
				maksimumHiz++;
			}

			// Eğer bir kavşaktaysak ve kavşaktan sağa veya sola dönmek için bir komut (input)
			// vermişsek bu inputu SonsuzYolScript'e (yolGenerator) ilet. Böylece eğer o yönde
			// dönüş yapmak mümkünse SonsuzYolScript infinite yolu o yönde devam ettirecek
			// ve karakteri o yönde döndürecek
			if( donemecteyiz && donusYonu != 0 )
			{
				if( donusYonu == -1 )
					yolGenerator.sol = true;
				else
					yolGenerator.sag = true;

				donemecteyiz = false;
				donusYonu = 0;
			}
		}
	}

	// Karakteri zıplatmaya yarayan fonksiyon
	public void Zipla()
	{
		// Eğer component disabled (kapalı) ise zıplama
		// ( mesela oyunun başında menü gözüküyorken kapalı)
		if( !enabled )
			return;

		// Eğer karakter hâlâ hayattaysa ve bir yerlerden aşağı düşmüyorsa karakteri zıplat
		if( !ucurum && !death && transform.localPosition.y <= 3f && Mathf.Abs( rb.velocity.y ) < 0.5f )
			rb.AddForce( new Vector3( 0f, ziplamaHizi, 0f ), ForceMode.Impulse );
	}

	// Karaktere, kavşağa gelince sola dönmesini söyleyen fonksiyon
	public void SolaDon()
	{
		// eğer karakter hâlâ hayattaysa
		if( !ucurum && !death )
		{
			// sola dönme inputunu ayarla
			donusYonu = -1;

			// 1 saniye sonra DonusYonunuResetle fonksiyonunu çağır
			// Bu fonksiyon donusYonu'nu geri 0 yapar (resetler).
			// Neden kavşaktan dönme inputunu 1 saniye sonra resetliyoruz?
			// Çünkü eğer Temple Run oynamışsanız bilirisiniz ki kavşaktan
			// dönmek için parmağı tam kavşağa varmak üzereyken sağa veya
			// sola doğru hareket ettirirsiniz; daha önce hareket ettirirseniz
			// input'unuz bir işe yaramaz. Burda da aynı şekilde, kavşaktan dönme
			// input'unu tam kavşağa varırken kabul ediyoruz, yolun ta başındayken
			// kavşaktan dönme inputu verilmişse o input'u 1 saniyenin ardından
			// yoksayıyoruz.
			// Peki CancelInvoke() fonksiyonu ne işe yarar? Daha önceden verilmiş olan
			// tüm Invoke komutlarını iptal eder. Diyelim ki kavşaktan sola dönme
			// input'unu bir kere verdikten hemen yarım saniye sonra bir daha veriyoruz.
			// Bu durumda verdiğimiz ilk input'tan toplam 1.5 saniye sonra kavşaktan dönme
			// inputunun resetlenmesi lazım. Ancak CancelInvoke() yapmazsak ilk input'u
			// verdiğimiz sırada çalışan Invoke komutu işlevini yapmaya devam eder ve ilk
			// input'tan 1, ikinci inputtan yarım saniye sonra kavşaktan dönme inputu 
			// resetlenir (oysa ki ilk input'tan 1.5, ikinci input'tan 1 saniye sonra 
			// resetlenmesi gerekiyordu)
			CancelInvoke();
			Invoke( "DonusYonunuResetle", 1f );
		}
	}

	// Karaktere, kavşağa gelince sağa dönmesini söyleyen fonksiyon
	public void SagaDon()
	{
		// eğer karakter hâlâ hayattaysa
		if( !ucurum && !death )
		{
			// sağa dönme inputunu ayarla
			donusYonu = 1;

			// 1 saniye sonra DonusYonunuResetle fonksiyonunu çağır
			// Bu fonksiyon donusYonu'nu geri 0 yapar (resetler).
			CancelInvoke();
			Invoke( "DonusYonunuResetle", 1f );
		}
	}

	public Vector3 KonumuResetle()
	{
		Vector3 kaydirmaMiktari = -transform.localPosition;
		kaydirmaMiktari.y = 0f;
		transform.localPosition += kaydirmaMiktari;

		if( yon == YolYonu.Ileri || yon == YolYonu.Geri )
		{
			limitMinDeger += kaydirmaMiktari.x;
			limitMaxDeger += kaydirmaMiktari.x;
		}
		else
		{
			limitMinDeger += kaydirmaMiktari.z;
			limitMaxDeger += kaydirmaMiktari.z;
		}

		return kaydirmaMiktari;
	}

	// Kavşaktan sağa veya sola dönme inputunu resetleyen fonksiyon
	private void DonusYonunuResetle()
	{
		donusYonu = 0;
	}

	// Oyun bitince yapılacak şeyler
	private void GameOver()
	{
		death = true;

		// eğer elde ettiğimiz skor yüksekskordan daha iyiyse bu skoru
		// yüksekskor olarak diske kaydet
		if( (int) skor > PlayerPrefs.GetInt( "YuksekSkor" ) )
		{
			PlayerPrefs.SetInt( "YuksekSkor", (int) skor );
			PlayerPrefs.Save();
		}

		// Game Over menüsünü aktif et 
		// (bu menü oyunun başında kapalı (inaktif) vaziyette)
		Arayuz.Instance.GameOverMenusunuGoster( (int) skor );

		// Oyun bitme sesini çal
		ses.PlayOneShot( gameOverSesi );
	}

	// Player, collider'ının Is Trigger'ı işaretli başka bir objeyle
	// "temas edince" bu fonksiyon çağrılır ve temas edilen obje
	// c isimli parametrede depolanır
	private void OnTriggerEnter( Collider c )
	{
		if( c.CompareTag( "Donemec" ) )
		{
			// Eğer kavşak objesinin temas alanıyla temas etmişsek:
			// kavşağa girdiğimizi belirleyen değişkeni true yap
			donemecteyiz = true;
		}
		else if( c.CompareTag( "PuanObjesi" ) )
		{
			// Eğer bir puan objesine dokunmuşsak:
			// skoru arttır
			skor += 10;
			Arayuz.Instance.OyunIcıSkoruGuncelle( (int) skor );

			// skor objesini inaktif yap
			c.gameObject.SetActive( false );

			// puan toplama sesi çal
			ses.PlayOneShot( puanSesi );
		}
	}

	// Player, collider'ının Is Trigger'ı işaretli başka bir objeyle
	// temasını "kesince" bu fonksiyon çağrılır ve teması kesilen obje
	// c isimli parametrede depolanır
	private void OnTriggerExit( Collider c )
	{
		// eğer kavşaktan çıkmışsak kavşak değişkenini false yap
		if( c.CompareTag( "Donemec" ) )
			donemecteyiz = false;
	}

	// Player, collider'ının Is Trigger'ı işaretli "olmayan" başka bir objeyle
	// "temas edince" bu fonksiyon çağrılır ve temas edilen obje
	// c isimli parametrede depolanır
	private void OnCollisionEnter( Collision c )
	{
		// Eğer oyun bittiyse teması yoksay
		if( death )
			return;

		// eğer temas edilen objenin tag'ı "OlumculEngel" ise
		// (bu tag'ı çarpılabilir duvar objelerine verdim):
		if( c.collider.CompareTag( "OlumculEngel" ) )
		{
			// Rigidbody'i kapat
			rb.isKinematic = true;
			// Karakterin ölme animasyonunu oynat
			anim.Play( "OlmeAnimasyonu" );
			// Karakter ölünce yapılacak işlemleri gerçekleştir
			GameOver();
		}
	}
}