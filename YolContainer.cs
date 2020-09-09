using UnityEngine;
using System.Collections.Generic;

public enum YolYonu { Ileri = 0, Sag = 1, Geri = 2, Sol = 3 };

// Düz bir yolun içeriğini depolayan script
// Düz bir yolda genelde birden çok zemin prefab'ının klonları ve
// birden çok puan objesi klonu yer alır
// Örneğin oyunun en başında elimizde düz bir yol, yolun ucunda iki yönlü
// bir kavşak ve kavşağın iki ucundan çıkan iki ayrı düz yol daha olsun.
// Bu durumda elimizde toplam 3 adet YolContainer objesi olmuş olur.
public class YolContainer
{
	// Bu yolda yer alan tüm zemin objelerini depolayan array (dizi)
	private List<YolObjesi> yol;

	// Bu yolda yer alan tüm puan objelerini depolayan list
	private List<Transform> puanObjeleri;

	// yolun bitiş noktası
	private Vector3 bitisNoktasi;

	// yolun yönü (ileri yön, sağ yön, geri yön veya sol yön)
	private YolYonu yon;

	// yolun ileri yön vektörü
	private Vector3 ileriYon;

	// yolun bitiş noktasını döndüren property
	public Vector3 BitisNoktasi { get { return bitisNoktasi; } }

	// yolun kaç zeminden oluştuğunu döndüren property
	public int Uzunluk { get { return yol.Count; } }

	// yolun yönünü döndüren property
	public YolYonu Yon { get { return yon; } }

	// yolun yönünü döndüren property
	public Vector3 IleriYon { get { return ileriYon; } }

	// yolun doğusuna denk gelen yönü döndüren property
	public Vector3 SagYon
	{
		get
		{
			if( yon == YolYonu.Ileri )
				return new Vector3( 1f, 0f, 0f );
			else if( yon == YolYonu.Sag )
				return new Vector3( 0f, 0f, -1f );
			else if( yon == YolYonu.Geri )
				return new Vector3( -1f, 0f, 0f );
			else
				return new Vector3( 0f, 0f, 1f );
		}
	}

	public YolContainer()
	{
		yol = new List<YolObjesi>( 32 );
		puanObjeleri = new List<Transform>( 32 );
	}

	// Düz yolu oluşturmaya yarayan fonksiyon:
	// - Yolda kullanacağımız zeminleri ve puan objelerini çekmek için havuzu kullanıyoruz
	// - baslangicNoktasi yolun hangi koordinattan başlayacağını belirler
	// - ileriYon yolun hangi yönde gideceğini belirler
	// - uzunluk yolun kaç zemin klonundan oluşacağını belirler
	public void YolOlustur( Vector3 baslangicNoktasi, YolYonu yolYonu, int uzunluk )
	{
		// yolYonu'nden yolun ileri yön vektörünü hesapla
		if( yolYonu == YolYonu.Ileri )
			ileriYon = new Vector3( 0f, 0f, 1f );
		else if( yolYonu == YolYonu.Sag )
			ileriYon = new Vector3( 1f, 0f, 0f );
		else if( yolYonu == YolYonu.Sol )
			ileriYon = new Vector3( -1f, 0f, 0f );
		else
			ileriYon = new Vector3( 0f, 0f, -1f );

		// yola dizeceğimiz zemin objelerinin sahip olacağı rotation'ı bulup egim'de depoluyoruz
		Vector3 egim = SonsuzYolScript.YolEgiminiBul( yolYonu );

		for( int i = 0; i < uzunluk; i++ )
		{
			// Havuzdan rastgele bir zemin objesi çek
			YolObjesi obje = ObjeHavuzu.Instance.HavuzdanYolObjesiCek();

			// bu zemin klonunun konumunu ve eğimini ayarlıyor, ardından zemini aktif hale getiriyoruz
			obje.transform.localPosition = baslangicNoktasi;
			obje.transform.localEulerAngles = egim;
			obje.gameObject.SetActive( true );

			// zemini ve zeminin çıktığı prefab'ın index'ini ilgili array'lerimizde depoluyoruz
			yol.Add( obje );

			// bir sonraki zemin klonunu bu zeminin uzunluğu kadar ileride oluşturuyoruz ki
			// sonraki zemin bu zeminin üzerinde oluşmasın
			baslangicNoktasi += ileriYon * obje.ebatlar.z;
		}

		// yolun bitiş noktasını ve yönünü ayarlıyoruz
		bitisNoktasi = baslangicNoktasi;
		yon = yolYonu;
	}

	// Düz yola puan objeleri dizmeye yarayan fonksiyon:
	// - puan objelerini havuzdan (pool) çekiyoruz
	// - artarda kaç puan objesi dizileceği bilgisini birDizidekiPuanObjesiSayisi vasıtasıyla dışarıdan alıyoruz
	public void YolaPuanObjeleriDiz( int birDizidekiPuanObjesiSayisi )
	{
		// iki dizi arasında kaç zeminlik aralık olacağını belirliyoruz
		// (aslında bu değeri de dışarıdan parametre olarak almam uygun olurdu)
		int puanObjeleriArasiAralik = 1;

		// 0- sol
		// 1- sağ
		// puan objelerinin yolun hangi tarafına dizileceğini belirleyen değişken
		int yon = Random.Range( 0, 2 );

		int diziliPuanObjesi = 0;
		for( int i = 0; i < yol.Count; i++ )
		{
			int puanSpawnNoktasiSayisi;
			Transform parentObje;

			// yolun puan objesi dizilebilecek noktalarını (spawn noktalarını) alıyoruz
			if( yon == 0 )
				parentObje = yol[i].solPuanSpawnNoktalari;
			else
				parentObje = yol[i].sagPuanSpawnNoktalari;

			// bu zeminde yer alan puan spawn noktası sayısını değişkende tutuyoruz
			puanSpawnNoktasiSayisi = parentObje.childCount;

			// Spawn noktalarının sonuna ulaşmadığımız ve bir dizi puan objesi dizmediğimiz müddetçe:
			for( int j = 0; j < puanSpawnNoktasiSayisi && diziliPuanObjesi < birDizidekiPuanObjesiSayisi; j++ )
			{
				// havuzdan (pool) bir puan objesi klonu çekiyoruz
				Transform obje = ObjeHavuzu.Instance.HavuzdanPuanObjesiCek();

				// puan objesini spawn noktasına konumlandırıyor ve aktif hale getiriyoruz
				obje.localPosition = parentObje.GetChild( j ).position;
				obje.localEulerAngles = new Vector3( 0f, 0f, 0f );
				obje.gameObject.SetActive( true );

				// puan objesini ilgili list'e ekliyoruz
				puanObjeleri.Add( obje );

				// bu dizideki puan objesi sayısını 1 artırıyoruz
				diziliPuanObjesi++;
			}

			// eğer ki diziyi tamamlamışsak (artarda birDizidekiPuanObjesiSayisi kadar
			// puan objesi spawn etmişsek):
			if( diziliPuanObjesi == birDizidekiPuanObjesiSayisi )
			{
				// diziyi sıfırla, puanObjeleriArasiAralik kadar zemini atla ve
				// yeni dizideki puanların spawn olacağı yönü tekrar rastgele bir şekilde seç
				diziliPuanObjesi = 0;
				i += puanObjeleriArasiAralik;
				yon = Random.Range( 0, 2 );
			}
		}
	}

	// Yolu ve üzerindeki pu an objelerini 3D uzayda belli bir miktar kaydırmaya yarayan fonksiyon
	public void YoluKaydir( Vector3 kaydirmaMiktari )
	{
		for( int i = 0; i < yol.Count; i++ )
			yol[i].transform.localPosition += kaydirmaMiktari;

		for( int i = 0; i < puanObjeleri.Count; i++ )
			puanObjeleri[i].localPosition += kaydirmaMiktari;

		bitisNoktasi += kaydirmaMiktari;
	}

	// Bu yolu yok ederken (oyuncu artık bu yolda koşmayı bitirip başka yola saptığında)
	// çağrılan ve yoldaki puan objeleri ile zemin objelerini havuza geri eklemeye yarayan
	// fonksiyon (yani Destroy yapmıyoruz)
	public void YoluYokEt()
	{
		// zemin objelerini deaktif et ve havuza ekle (bu esnada hangi zemin objesi klonunun
		// hangi prefab'tan çıktığını dikkate al (yolIndexler vasıtasıyla))
		for( int i = 0; i < yol.Count; i++ )
		{
			YolObjesi obje = yol[i];
			obje.gameObject.SetActive( false );
			ObjeHavuzu.Instance.HavuzaYolObjesiEkle( obje );
		}

		// puan objelerini deaktif et ve havuza ekle
		for( int i = 0; i < puanObjeleri.Count; i++ )
		{
			Transform obje = puanObjeleri[i];
			obje.gameObject.SetActive( false );
			ObjeHavuzu.Instance.HavuzaPuanObjesiEkle( obje );
		}

		// List'lerin içini boşalt
		yol.Clear();
		puanObjeleri.Clear();
	}
}