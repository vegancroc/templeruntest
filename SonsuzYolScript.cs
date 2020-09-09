using UnityEngine;
using System.Collections;

// Infinite road yapmayı görev edinmiş kahraman script!
// Oyunun en başında ufak bir yol rastgele bir şekilde oluşturulur 
// ve karakter kavşaklardan sağa/sola saptıkça yolun devamı bu
// script vasıtasıyla oluşturulmaya devam edilir. Böylece infinite road
// atmosferi oluşturulur oysa Scene panelinden bakılacak olursa oyun
// ilerledikçe eski yolun yok olup da yeni yolun sonradan oluşturulduğu
// kolayca gözlemlenebilir
// Bu script düzgün çalışmak için ObjeHavuzu scriptine (havuz)(pool) ihtiyaç
// duyar (RequireComponent)
[RequireComponent( typeof( ObjeHavuzu ) )]
public class SonsuzYolScript : MonoBehaviour
{
	// Düz bir yolun sahip olacağı minimum zemin objesi sayısı
	public int yolMinimumUzunluk = 10;
	// Düz bir yolun sahip olacağı maksimum zemin objesi sayısı
	public int yolMaksimumUzunluk = 20;
	// Puan objeleri yolda birbiri ardına dizilirler (dizi şeklinde) ve bu
	// değişken bir dizinin sahip olacağı puan objesi sayısını depolar
	public int ardArdaDiziliPuanObjesiSayisi = 6;

	// karakterin üzerinde koşmakta olduğu yol (ileri yöndeki yol)
	private YolContainer ileriYol;
	// ileriYol'un ucundaki kavşaktan sağa sapınca gireceğimiz yol
	private YolContainer sagYol;
	// ileriYol'un ucundaki kavşaktan sola sapınca gireceğimiz yol
	private YolContainer solYol;
	// ileriYol'un ucundaki kavşak objesi
	private YolObjesi kavsakObjesi;

	[System.NonSerialized]
	public bool sol = false;
	[System.NonSerialized]
	public bool sag = false;

	private void Start()
	{
		// Rastgele bir yol oluştur
		ileriYol = new YolContainer();
		sagYol = new YolContainer();
		solYol = new YolContainer();

		YolOlustur( ileriYol, new Vector3( 0f, 0f, 15f ), YolYonu.Ileri );
		DonemecOlustur( ileriYol.BitisNoktasi, YolYonu.Ileri );
	}

	// Random bir şekilde düz bir yol oluşturmaya yarayan fonksiyon
	private void YolOlustur( YolContainer c, Vector3 baslangicNoktasi, YolYonu yolYonu )
	{
		// yolun uzunluğunu rastgele olarak belirle
		int yolUzunluk = Random.Range( yolMinimumUzunluk, yolMaksimumUzunluk + 1 );

		// yolu oluştur (yola zeminleri dik)
		c.YolOlustur( baslangicNoktasi, yolYonu, yolUzunluk );
		// yola puan objelerini diz
		c.YolaPuanObjeleriDiz( ardArdaDiziliPuanObjesiSayisi );
	}

	// Kavşak ve bu kavşağa bağlı düz yollar oluşturmaya yarayan fonksiyon
	private void DonemecOlustur( Vector3 baslangicNoktasi, YolYonu yolYonu )
	{
		// Kavşak objesinin sahip olacağı rotation değerini bul
		Vector3 egim = YolEgiminiBul( yolYonu );

		// [0-2] aralığında rastgele bir integer döndürülür ve:
		// 0- sol kavşak oluşturulur
		// 1- sağ kavşak oluşturulur
		// 2- iki yönlü kavşak oluşturulur
		switch( Random.Range( 0, 3 ) )
		{
			case 0:
				// sadece sola dönemeç oluştur
				// sol kavşak objesini havuzdan çek ve oyun alanına yerleştir
				kavsakObjesi = ObjeHavuzu.Instance.HavuzdanSolDonemecObjesiCek();
				kavsakObjesi.transform.localPosition = baslangicNoktasi;
				kavsakObjesi.transform.localEulerAngles = egim;
				kavsakObjesi.gameObject.SetActive( true );

				// sola dön
				if( yolYonu == YolYonu.Ileri )
				{
					yolYonu = YolYonu.Sol;
					baslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.x / 2, 0, kavsakObjesi.ebatlar.z / 2 );
				}
				else if( yolYonu == YolYonu.Sol )
				{
					yolYonu = YolYonu.Geri;
					baslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.z / 2, 0, -kavsakObjesi.ebatlar.x / 2 );
				}
				else if( yolYonu == YolYonu.Geri )
				{
					yolYonu = YolYonu.Sag;
					baslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.x / 2, 0, -kavsakObjesi.ebatlar.z / 2 );
				}
				else
				{
					yolYonu = YolYonu.Ileri;
					baslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.z / 2, 0, kavsakObjesi.ebatlar.x / 2 );
				}

				// kavşağın ucunda yeni bir düz yol oluştur
				YolOlustur( solYol, baslangicNoktasi, yolYonu );
				break;
			case 1:
				// sadece sağa dönemeç oluştur
				// sağ kavşak objesini havuzdan çek ve oyun alanına yerleştir
				kavsakObjesi = ObjeHavuzu.Instance.HavuzdanSagDonemecObjesiCek();
				kavsakObjesi.transform.localPosition = baslangicNoktasi;
				kavsakObjesi.transform.localEulerAngles = egim;
				kavsakObjesi.gameObject.SetActive( true );

				// sağa dön
				if( yolYonu == YolYonu.Ileri )
				{
					yolYonu = YolYonu.Sag;
					baslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.x / 2, 0, kavsakObjesi.ebatlar.z / 2 );
				}
				else if( yolYonu == YolYonu.Sol )
				{
					yolYonu = YolYonu.Ileri;
					baslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.z / 2, 0, kavsakObjesi.ebatlar.x / 2 );
				}
				else if( yolYonu == YolYonu.Geri )
				{
					yolYonu = YolYonu.Sol;
					baslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.x / 2, 0, -kavsakObjesi.ebatlar.z / 2 );
				}
				else
				{
					yolYonu = YolYonu.Geri;
					baslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.z / 2, 0, -kavsakObjesi.ebatlar.x / 2 );
				}

				// kavşağın ucunda yeni bir düz yol oluştur
				YolOlustur( sagYol, baslangicNoktasi, yolYonu );
				break;
			case 2:
				// hem sola hem sağa dönemeç oluştur
				// iki yönlü kavşak objesini havuzdan çek ve oyun alanına yerleştir
				kavsakObjesi = ObjeHavuzu.Instance.HavuzdanSolVeSagDonemecObjesiCek();
				kavsakObjesi.transform.localPosition = baslangicNoktasi;
				kavsakObjesi.transform.localEulerAngles = egim;
				kavsakObjesi.gameObject.SetActive( true );

				// hem sol hem de sağ yönü birer değişkende depola
				YolYonu tersYolYonu;
				Vector3 tersYolBaslangicNoktasi = baslangicNoktasi;
				if( yolYonu == YolYonu.Ileri )
				{
					yolYonu = YolYonu.Sol;
					tersYolYonu = YolYonu.Sag;
					baslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.x / 2, 0, kavsakObjesi.ebatlar.z / 2 );
					tersYolBaslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.x / 2, 0, kavsakObjesi.ebatlar.z / 2 );
				}
				else if( yolYonu == YolYonu.Sol )
				{
					yolYonu = YolYonu.Geri;
					tersYolYonu = YolYonu.Ileri;
					baslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.z / 2, 0, -kavsakObjesi.ebatlar.x / 2 );
					tersYolBaslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.z / 2, 0, kavsakObjesi.ebatlar.x / 2 );
				}
				else if( yolYonu == YolYonu.Geri )
				{
					yolYonu = YolYonu.Sag;
					tersYolYonu = YolYonu.Sol;
					baslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.x / 2, 0, -kavsakObjesi.ebatlar.z / 2 );
					tersYolBaslangicNoktasi += new Vector3( -kavsakObjesi.ebatlar.x / 2, 0, -kavsakObjesi.ebatlar.z / 2 );
				}
				else
				{
					yolYonu = YolYonu.Ileri;
					tersYolYonu = YolYonu.Geri;
					baslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.z / 2, 0, kavsakObjesi.ebatlar.x / 2 );
					tersYolBaslangicNoktasi += new Vector3( kavsakObjesi.ebatlar.z / 2, 0, -kavsakObjesi.ebatlar.x / 2 );
				}

				// kavşağın iki ucunda da yeni birer düz yol oluştur
				YolOlustur( solYol, baslangicNoktasi, yolYonu );
				YolOlustur( sagYol, tersYolBaslangicNoktasi, tersYolYonu );
				break;
		}
	}

	private void Update()
	{
		YolContainer yeniIleriYol;
		if( sol ) // Eğer ileriYol'un ucundaki kavşaktan sola dönme talimatı verilmişse
			yeniIleriYol = solYol;
		else if( sag ) // Eğer ileriYol'un ucundaki kavşaktan sağa dönme talimatı verilmişse
			yeniIleriYol = sagYol;
		else
			yeniIleriYol = null;

		// eğer elimizde geçerli bir yol varsa (yani bu kavşaktan ilgili yöne dönüş mümkünse):
		if( yeniIleriYol != null && yeniIleriYol.Uzunluk > 0 )
		{
			// player'ın yön değişkenlerini ayarla
			Player player = Player.Instance;
			player.transform.localEulerAngles = YolEgiminiBul( yeniIleriYol.Yon );
			player.yon = yeniIleriYol.Yon;
			player.ileriYon = yeniIleriYol.IleriYon;
			player.sagYon = yeniIleriYol.SagYon;

			// player'ın yatay eksende gidebileceği minimum ve maksimum limitleri
			// ayarla (konseptle alakalı detaylı açıklama Player scriptinde mevcut)
			Vector3 yolBitisNoktasi = yeniIleriYol.BitisNoktasi;
			if( yeniIleriYol.Yon == YolYonu.Ileri || yeniIleriYol.Yon == YolYonu.Geri )
			{
				player.limitMinDeger = yolBitisNoktasi.x - 2.75f;
				player.limitMaxDeger = yolBitisNoktasi.x + 2.75f;
			}
			else
			{
				player.limitMinDeger = yolBitisNoktasi.z - 2.75f;
				player.limitMaxDeger = yolBitisNoktasi.z + 2.75f;
			}

			// sol yolu artık ileri yol (üzerinde koşulan yol) olarak ata
			YolContainer temp = ileriYol;
			ileriYol = yeniIleriYol;

			if( sol )
				solYol = temp;
			else
				sagYol = temp;

			// yolun devamını oluştur
			StartCoroutine( YoluGuncelle() );
		}

		sol = false;
		sag = false;
	}

	// Mevcut yolun ucuna yeni bir kavşak koyup bu kavşağın ucundan
	// yeni düz yol(lar) çıkarmaya yarayan fonksiyon
	private IEnumerator YoluGuncelle()
	{
		// 2 saniye bekle çünkü player kavşaktan tam döndüğü anda 
		// eski yolu yok edersek player büyük olasılıkla aşağı düşer
		yield return new WaitForSeconds( 2f );

		// arkada kalan yolları yok et (içeriklerini havuza yolla)
		solYol.YoluYokEt();
		sagYol.YoluYokEt();

		// az önce döndüğümüz kavşağı havuza ekle
		kavsakObjesi.gameObject.SetActive( false );
		ObjeHavuzu.Instance.HavuzaYolObjesiEkle( kavsakObjesi );

		// Tüm objelerin konumlarını, 3D uzayda 0,0,0'a yaklaşacak şekilde kaydır
		// çünkü objelerin konumları 0,0,0'dan çok uzaklaşınca, "floating point precision"
		// diye donanımsal bir kısıtlamadan dolayı objeler arası olmaması gereken boşluklar
		// olabiliyor veya kamera hareketi sapıtabiliyor
		Vector3 kaydirmaMiktari = Player.Instance.KonumuResetle();
		KameraKontrol.Instance.transform.localPosition += kaydirmaMiktari;
		ileriYol.YoluKaydir( kaydirmaMiktari );

		// yolun ucuna yeni bir kavşak ve o kavşaktan çıkan yeni
		// yol(lar) oluştur
		DonemecOlustur( ileriYol.BitisNoktasi, ileriYol.Yon );
	}

	// Girilen yönde dizilen zemin objelerinin sahip olması gereken eğimi
	// bulmaya yarayan fonksiyon
	public static Vector3 YolEgiminiBul( YolYonu yolYonu )
	{
		if( yolYonu == YolYonu.Ileri )
			return new Vector3( 0f, 0f, 0f );
		else if( yolYonu == YolYonu.Sag )
			return new Vector3( 0f, 90f, 0f );
		else if( yolYonu == YolYonu.Sol )
			return new Vector3( 0f, 270f, 0f );
		else
			return new Vector3( 0f, 180f, 0f );
	}
}