using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

public interface ISerialize
{
	void OnBeforeSerialize();
	void OnAfterDeserialize();
}

public static class Serializer 
{
	public static T Load<T>(string filename) where T: ISerialize
	{
		T tempVar = default(T);

		if(!FileExists(filename))
			return tempVar;

		tempVar = LoadWin<T>(filename);

		if(tempVar != null)
			tempVar.OnAfterDeserialize();

		return tempVar;
	}

	public static void Save<T>(string filename, T data) where T: ISerialize
	{	
		data.OnBeforeSerialize();
		SaveWin<T>(filename, data);
	}

	static void SaveListInternal<T>(string filename, List<T> data) where T : ISerialize
	{
		SList<T> saveableList = new SList<T>();
		saveableList.list = data;
		saveableList.OnBeforeSerialize();
		SaveWin<SList<T>>(filename, saveableList);
	}

	public static void SaveArrayInternal<T>(string filename, T[] data) where T : ISerialize
	{
		SArray<T> saveableArr = new SArray<T>();
		saveableArr.arr = data;
		saveableArr.OnBeforeSerialize();
		SaveWin<SArray<T>>(filename, saveableArr);
	}

	public static void Save<T>(string filename, List<T> data)
	{
		bool hasInterface = typeof(T).GetInterface("ISerialize") != null;

		if(hasInterface)
		{
			SaveListInternal(filename, data as List<ISerialize>);
		}
		else
		{
			SList<SVar<T>> saveableList = new SList<SVar<T>>();
			for(int i = 0; i < data.Count; i++)
			{
				saveableList.list.Add(new SVar<T>(data[i]));
			}
				
			saveableList.OnBeforeSerialize();
			SaveWin<SList<SVar<T>>>(filename, saveableList);
		}
	}

	public static void Save<T>(string filename, T[] data)
	{
		bool hasInterface = typeof(T).GetInterface("ISerialize") != null;

		if(hasInterface)
		{
			SaveArrayInternal(filename, data as ISerialize[]);
		}
		else
		{
			SArray<SVar<T>> saveableArr = new SArray<SVar<T>>();
			saveableArr.arr = new SVar<T>[data.Length];

			for(int i = 0; i < data.Length; i++)
			{
				saveableArr.arr[i] = new SVar<T>(data[i]);
			}

			saveableArr.OnBeforeSerialize();
			SaveWin<SArray<SVar<T>>>(filename, saveableArr);
		}
	}

	public static void Delete(string filename)
	{
		string filePath = Application.streamingAssetsPath + "/" + filename;
		File.Delete(filePath);
	}

	public static bool FileExists(string filename){
		return ExistsWin(filename);
	}

	private static T LoadWin<T>(string filename) where T: ISerialize{
		string filePath = Application.streamingAssetsPath + "/" + filename;
		XmlSerializer tempXmlSeri = new XmlSerializer(typeof(T));
		Stream tempStreamRead;
		tempStreamRead = File.OpenText(filePath).BaseStream;
		T loadedData = (T)tempXmlSeri.Deserialize(tempStreamRead);
		tempStreamRead.Close();
		return loadedData;
	}

	private static void SaveWin<T>(string filename, T data) where T: ISerialize{	
		string filePath = Application.streamingAssetsPath + "/" + filename;

		XmlSerializer tempXmlSeri = new XmlSerializer(typeof(T));
		Stream tempStream;
		tempStream = File.Create (filePath);
		//Used to encode the file to a windows phone 8 acceptable format
		XmlTextWriter tempTextWrite = new XmlTextWriter(tempStream, Encoding.UTF8);
		tempXmlSeri.Serialize(tempTextWrite, data);
		tempTextWrite.Close();
	}

	private static bool ExistsWin(string filename){
		string filePath = Application.streamingAssetsPath + "/" + filename;

		return File.Exists (filePath);
	}
}

[XmlRoot("SVar")]
public class SVar<T> : ISerialize
{
	[XmlAttribute("var")]
	public T var;

	public SVar()
	{
		var = default(T);
	}

	public SVar(T newVar)
	{
		var = newVar;
	}

	public void OnBeforeSerialize()
	{

	}

	public void OnAfterDeserialize()
	{

	}
}

[XmlRoot("SArray")]
public class SArray<T> : ISerialize where T : ISerialize
{
	[XmlArray("Array"),XmlArrayItem("Element")]
	public T[] arr;

	public SArray()
	{
		arr = null;
	}

	public SArray(int length)
	{
		arr = new T[length];
	}

	public void OnBeforeSerialize()
	{
		if(arr == null)
			return;

		for(int i = 0; i < arr.Length; i++)
		{
			arr[i].OnBeforeSerialize();
		}
	}

	public void OnAfterDeserialize()
	{
		if(arr == null)
			return;

		for(int i = 0; i < arr.Length; i++)
		{
			arr[i].OnAfterDeserialize();
		}
	}
}

[XmlRoot("SList")]
public class SList<T> : ISerialize where T : ISerialize
{
	[XmlIgnore]
	public List<T> list;
	[XmlArray("Array"),XmlArrayItem("Element")]
	T[] arr;

	public SList()
	{
		list = new List<T>();
	}

	public void OnBeforeSerialize()
	{
		if(list == null)
			return;

		arr = new T[list.Count];
		for(int i = 0; i < list.Count; i++)
		{
			arr[i] = list[i];
			arr[i].OnBeforeSerialize();
		}
	}

	public void OnAfterDeserialize()
	{
		if(arr == null)
		{
			list = new List<T>();
			return;
		}

		arr = new T[list.Count];
		for(int i = 0; i < arr.Length; i++)
		{
			list.Add(arr[i]);
			list[i].OnAfterDeserialize();
		}
	}
}