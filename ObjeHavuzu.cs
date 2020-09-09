using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/* 
 * Bu örnekte pooling adı verilen bir teknikten (pattern) faydalanıyoruz.
 * Bu pattern'in hedefi oldukça basit: eğer ki bir objeyi oyun boyunca
 * defalarca kez Instantiate ve Destroy ediyorsak bunun yerine o objeyi
 * Destroy etmiyor ama havuz adı verilen bir yerde depoluyoruz ve
 * o objeye tekrar ihtiyacımız olduğunda objeyi direkt havuzdan çekiyoruz,
 * yani Instantiate ile uğraşmıyoruz.
 *
 * Bu pattern bir infinite runner oyunu için kritik öneme sahip çünkü
 * bu tür oyunlarda yol prosedürel olarak oluşturuluyor, sonsuza kadar gidiyor
 * ama genel olarak aynı obje oyun boyunca defalarca kez kullanılıyor. Eğer ki
 * yeni yol oluşturma işlemini Instantiate ve Destroy'lar ile yapsaydık tek bir
 * seferde belki onlarca, belki yüzlerce objenin Instantiate ve(ya) Destroy edilmesi
 * gerekecekti ve bu da FPS'te o an ciddi bir düşüşe, belki de bir iki saniyelik ciddi
 * takılmalara sebep olacaktı.
*/

// Infinite yolumuzda defalarca kullandığımız objeleri depolayan havuz scripti
public class ObjeHavuzu : MonoBehaviour
{
	// bu script'e her yerden erişmek için property
	public static ObjeHavuzu Instance { get; private set; }

	// puan prefab'ları
	public Transform[] puanPrefablari;
	public int puanHavuzuBoyutu = 50;

	// zemin prefab'ları
	public YolObjesi[] ileriYolPrefablari;
	public int ileriYolHavuzuBoyutu = 75;

	// sadece sola dönmeye yarayan kavşak objesi prefab'ları
	public YolObjesi[] solDonemecPrefablari;
	public int solDonemecHavuzuBoyutu = 1;

	// sadece sağa dönmeye yarayan kavşak objesi prefab'ları
	public YolObjesi[] sagDonemecPrefablari;
	public int sagDonemecHavuzuBoyutu = 1;

	// hem sola hem de sağa dönmeye yarayan (gidilecek yönü oyuncu seçer) kavşak objesi prefab'ları
	public YolObjesi[] solVeSagDonemecPrefablari;
	public int solVeSagDonemecHavuzuBoyutu = 1;

	// - HAVUZ -
	// Puan objelerini depolayan havuz (ileriYolObjeleriHavuzu)
	// Bu değişkenin türüne dikkat edin: List<Transform>
	// List<Transform> dediğimiz şey Transform türündeki bir List'tir
	// List veri türü, array gibi içerisinde birden çok objeyi depolamaya yarar
	// List'in array'den en önemli farkı, List'in boyutunun (Count) belli olmamasıdır
	// Yani List'e istediğimiz sayıda obje atabiliriz ama bunu array'de yapamayız
	private List<Transform> puanObjeleriHavuzu;

	private List<YolObjesi> ileriYolObjeleriHavuzu;
	private List<YolObjesi> solDonemecHavuzu;
	private List<YolObjesi> sagDonemecHavuzu;
	private List<YolObjesi> solVeSagDonemecHavuzu;
	// - HAVUZ -

	private void Awake()
	{
		if( Instance == null )
			Instance = this;
		else if( this != Instance )
		{
			Destroy( this );
			return;
		}

		// Oyunun en başında havuzları hatırı sayılır miktarda objeyle doldur
		puanObjeleriHavuzu = new List<Transform>( puanHavuzuBoyutu );
		HavuzuDoldur( puanObjeleriHavuzu, puanPrefablari, puanHavuzuBoyutu );

		ileriYolObjeleriHavuzu = new List<YolObjesi>( ileriYolHavuzuBoyutu );
		HavuzuDoldur( ileriYolObjeleriHavuzu, ileriYolPrefablari, ileriYolHavuzuBoyutu );

		solDonemecHavuzu = new List<YolObjesi>( solDonemecHavuzuBoyutu );
		HavuzuDoldur( solDonemecHavuzu, solDonemecPrefablari, solDonemecHavuzuBoyutu );

		sagDonemecHavuzu = new List<YolObjesi>( sagDonemecHavuzuBoyutu );
		HavuzuDoldur( sagDonemecHavuzu, sagDonemecPrefablari, sagDonemecHavuzuBoyutu );

		solVeSagDonemecHavuzu = new List<YolObjesi>( solVeSagDonemecHavuzuBoyutu );
		HavuzuDoldur( solVeSagDonemecHavuzu, solVeSagDonemecPrefablari, solVeSagDonemecHavuzuBoyutu );
	}

	// Girilen havuzu prefab klonları ile doldurmaya yarayan fonksiyon
	private void HavuzuDoldur( IList havuz, IList prefablar, int havuzBoyutu )
	{
		// Havuzda her prefab'dan eşit miktarda olsun
		int herTipObjeSayisi = havuzBoyutu / prefablar.Count;

		for( int i = 0; i < prefablar.Count; i++ )
		{
			// Her prefab'dan eşit miktarda Instantiate edip klonu havuza ekle
			for( int j = 0; j < herTipObjeSayisi; j++ )
			{
				Component obje = Instantiate( (Component) prefablar[i] );
				obje.gameObject.SetActive( false );
				havuz.Add( obje );
			}
		}
	}

	// Girilen havuzdan bir obje çekmeye yarayan fonksiyon
	private Component HavuzdanObjeCek( IList havuz, IList prefablar )
	{
		Component obje;

		if( havuz.Count <= 0 )
		{
			// Eğer ki havuz boşsa yeni bir klon oluştur ve onu döndür

			int randomIndex = Random.Range( 0, prefablar.Count );
			obje = Instantiate( (Component) prefablar[randomIndex] );
		}
		else
		{
			// Eğer ki havuz boş değilse, o havuzdan rastgele bir
			// elemanını döndür ve o elemanı havuzdan sil
			// Neden çektiğimiz elemanı List'ten (havuz) siliyoruz? Çünkü
			// eğer elemanı silmezsek, havuzdan yeni bir eleman çekerken yine
			// aynı objeyi döndürme olasılığımız olur ve bu da o yol objesinin aniden tekrar
			// konumlandırılması anlamına gelir. Biz bunu istemiyoruz. Biz o yol
			// objesine tekrar erişmenin ancak o yol objesiyle işimiz bittiğinde
			// gerçekleşebilmesini istiyoruz

			int randomIndex = Random.Range( 0, havuz.Count );
			obje = (Component) havuz[randomIndex];
			havuz.RemoveAt( randomIndex );
		}

		return obje;
	}

	public Transform HavuzdanPuanObjesiCek()
	{
		return (Transform) HavuzdanObjeCek( puanObjeleriHavuzu, puanPrefablari );
	}

	public YolObjesi HavuzdanYolObjesiCek()
	{
		return (YolObjesi) HavuzdanObjeCek( ileriYolObjeleriHavuzu, ileriYolPrefablari );
	}

	public YolObjesi HavuzdanSolDonemecObjesiCek()
	{
		return (YolObjesi) HavuzdanObjeCek( solDonemecHavuzu, solDonemecPrefablari );
	}

	public YolObjesi HavuzdanSagDonemecObjesiCek()
	{
		return (YolObjesi) HavuzdanObjeCek( sagDonemecHavuzu, sagDonemecPrefablari );
	}

	public YolObjesi HavuzdanSolVeSagDonemecObjesiCek()
	{
		return (YolObjesi) HavuzdanObjeCek( solVeSagDonemecHavuzu, solVeSagDonemecPrefablari );
	}

	// Bir puan objesini havuza eklemeye yarayan fonksiyon
	public void HavuzaPuanObjesiEkle( Transform obje )
	{
		// Bir List'e yeni bir eleman eklerken Add fonksiyonu kullanılır
		puanObjeleriHavuzu.Add( obje );
	}

	// Bir yol objesini ilgili havuza eklemeye yarayan fonksiyon
	public void HavuzaYolObjesiEkle( YolObjesi obje )
	{
		if( obje.yolTuru == YolTuru.DuzYol )
			ileriYolObjeleriHavuzu.Add( obje );
		else if( obje.yolTuru == YolTuru.SagDonemec )
			sagDonemecHavuzu.Add( obje );
		else if( obje.yolTuru == YolTuru.SolDonemec )
			solDonemecHavuzu.Add( obje );
		else
			solVeSagDonemecHavuzu.Add( obje );
	}
}