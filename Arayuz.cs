using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Ana menü UI objesinin faydalandığı script
public class Arayuz : MonoBehaviour
{
	// bu script'e her yerden erişmek için property
	public static Arayuz Instance { get; private set; }

	// oyun başladığında enabled edilecek scriptler
	public KameraKontrol kameraKontrol;
	public Animation kameraAnimation;

	public GameObject anaMenu;
	public GameObject oyunIciUI;
	public GameObject gameOverMenu;

	// credits yazısını yazdıran UI objesi
	public Text creditsText;

	// skor yazıları
	public Text oyunIciSkorText;
	public Text gameOverSkorText;

	// Oyun içi arayüzü saydam yapmaya yarayan component
	public CanvasGroup oyunIciCanvasGroup;

	// butonlara tıklayınca çıkan ses
	public AudioSource sesCalar;
	public AudioClip butonClickSesi;

	// Menü ekrandayken gözüken zemin objeleri
	public GameObject[] baslangicZeminler;

	private void Awake()
	{
		if( Instance == null )
			Instance = this;
		else if( this != Instance )
		{
			Destroy( this );
			return;
		}

		// Önceki oyun bitiminde timeScale 0 yapıldıysa diye
		// menüye giriş yapınca timeScale'i tekrar 1 yap
		Time.timeScale = 1f;

		// ana menüyü göster
		anaMenu.SetActive( true );
	}

	private void Start()
	{
		kameraKontrol.enabled = false;
		Player.Instance.enabled = false;
	}

	// Oyunu Başlat butonuna tıklayınca yapılacaklar
	public void OyunuBaslat()
	{
		// enable edilmesi gereken scriptleri enable et
		kameraKontrol.enabled = true;
		Player.Instance.enabled = true;
		Player.Instance.HareketeBasla();

		// kameranın loop halinde tekrarladığı animasyona son ver
		Destroy( kameraAnimation );

		// Oyunun başında scene'de yer alan BaslangicZemin objelerini yok et
		for( int i = 0; i < baslangicZeminler.Length; i++ )
			Destroy( baslangicZeminler[i], 4f );

		// butona tıklama sesi çal
		sesCalar.PlayOneShot( butonClickSesi );

		// ana menüyü kapat
		anaMenu.SetActive( false );

		// oyun içi arayüzü aç
		oyunIciUI.SetActive( true );
	}

	// Restart butonuna tıklayınca yapılacaklar
	public void Restart()
	{
		// Bölüme restart at (ana menü bölümün en başında gösteriliyor)
		SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
	}

	// Yapımcı butonuna tıklayınca yapılacaklar
	public void Credits()
	{
		// credits butonunun yazısını değiştir
		creditsText.text = "yasirkula.com";

		// 1 saniye sonra credits butonunun yazısını eski haline getir
		// ( CreditsDuzelt() fonksiyonu ve Invoke() fonksiyonu vasıtasıyla )
		CancelInvoke();
		Invoke( "CreditsDuzelt", 1f );

		// butona tıklama sesi çal
		sesCalar.PlayOneShot( butonClickSesi );
	}

	// Çıkış butonuna tıklayınca yapılacaklar
	public void Cikis()
	{
		// butona tıklama sesi çal
		sesCalar.PlayOneShot( butonClickSesi );

		// oyunu kapat
		Application.Quit();
	}

	public void GameOverMenusunuGoster( int skor )
	{
		gameOverMenu.SetActive( true );
		gameOverSkorText.text = "Skor : " + skor + "\nYüksekskor : " + PlayerPrefs.GetInt( "YuksekSkor" );
	}

	public void OyunIcıSkoruGuncelle( int skor )
	{
		// skoru çizdiren UI elemanını güncelle
		// burada skor'u <color=yellow> </color> etiketleri arasına alarak
		// UI elemanında skor'un sarı renkle çizdirilmesini sağlıyoruz
		oyunIciSkorText.text = "Score: <color=yellow>" + skor + "</color>";
	}

	private void CreditsDuzelt()
	{
		// credits butonunun yazısını eski haline getir
		creditsText.text = "Yapımcı";
	}

	// Karakter aşağı düşünce yazının saydam olmasını sağlayan coroutine
	public IEnumerator SkorYazisiniSaydamYap()
	{
		float saydamlik = 1f;
		while( saydamlik > 0f )
		{
			saydamlik -= Time.unscaledDeltaTime * 4f;
			oyunIciCanvasGroup.alpha = saydamlik;

			yield return null;
		}

		oyunIciCanvasGroup.alpha = 0f;
	}
}