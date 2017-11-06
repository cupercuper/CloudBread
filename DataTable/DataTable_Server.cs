using System;
using System.Collections.Generic;
using System.IO;
using System.Data;

public class DataTableBase
{
}

public class DataTableListBase
{
	public Dictionary<ulong,DataTableBase> DataList = new Dictionary<ulong,DataTableBase>();
	public virtual void Load(DataTable dataTable)
	{
	}

	public UInt16 Version = 0;
}

public class BossDataTable_List : DataTableListBase
{
	public const string NAME = "Boss";
	public const string DATAFILENAME = "BossData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			BossDataTable data = new BossDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class BossDataTable : DataTableBase
{
	public string Name;
	public string PrefabName;
	public int HP;
	public int Defence;
	public int MoveSpeed;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		HP = int.Parse(dataRow[3].ToString());
		Defence = int.Parse(dataRow[4].ToString());
		MoveSpeed = int.Parse(dataRow[5].ToString());
		Description = dataRow[6].ToString();
	}
}

public class CaptianDataTable_List : DataTableListBase
{
	public const string NAME = "Captian";
	public const string DATAFILENAME = "CaptianData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			CaptianDataTable data = new CaptianDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class CaptianDataTable : DataTableBase
{
	public string Name;
	public byte Type;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Type = byte.Parse(dataRow[2].ToString());
		Description = dataRow[3].ToString();
	}
}

public class EnemyDataTable_List : DataTableListBase
{
	public const string NAME = "Enemy";
	public const string DATAFILENAME = "EnemyData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			EnemyDataTable data = new EnemyDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class EnemyDataTable : DataTableBase
{
	public string Name;
	public string PrefabName;
	public int HP;
	public int DefencePower;
	public int MoveSpeed;
	public byte MoveType;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		HP = int.Parse(dataRow[3].ToString());
		DefencePower = int.Parse(dataRow[4].ToString());
		MoveSpeed = int.Parse(dataRow[5].ToString());
		MoveType = byte.Parse(dataRow[6].ToString());
		Description = dataRow[7].ToString();
	}
}

public class ProjectileDataTable_List : DataTableListBase
{
	public const string NAME = "Projectile";
	public const string DATAFILENAME = "ProjectileData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ProjectileDataTable data = new ProjectileDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ProjectileDataTable : DataTableBase
{
	public string Name;
	public string PrefabName;
	public int Speed;
	public int RotateSpeed;
	public string DestroyEffect;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		Speed = int.Parse(dataRow[3].ToString());
		RotateSpeed = int.Parse(dataRow[4].ToString());
		DestroyEffect = dataRow[5].ToString();
	}
}

public class StageDataTable_List : DataTableListBase
{
	public const string NAME = "Stage";
	public const string DATAFILENAME = "StageData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			StageDataTable data = new StageDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class StageDataTable : DataTableBase
{
	public string Name;
	public string Description;
	public ulong WaveStartSerialNo;
	public int WaveCoolTime;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Description = dataRow[2].ToString();
		WaveStartSerialNo = ulong.Parse(dataRow[3].ToString());
		WaveCoolTime = int.Parse(dataRow[4].ToString());
	}
}

public class UnitDataTable_List : DataTableListBase
{
	public const string NAME = "Unit";
	public const string DATAFILENAME = "UnitData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			UnitDataTable data = new UnitDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class UnitDataTable : DataTableBase
{
	public string Name;
	public string PrefabName;
	public byte Type;
	public byte Grade;
	public int AttackCoolTime;
	public int AttackRange;
	public int AttackPower;
	public int SplashRange;
	public int Size;
	public string Description;
	public bool Flip_90;
	public bool Flip_180;
	public bool Flip_270;
	public bool Flip_360;
	public ulong Projectile;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		Type = byte.Parse(dataRow[3].ToString());
		Grade = byte.Parse(dataRow[4].ToString());
		AttackCoolTime = int.Parse(dataRow[5].ToString());
		AttackRange = int.Parse(dataRow[6].ToString());
		AttackPower = int.Parse(dataRow[7].ToString());
		SplashRange = int.Parse(dataRow[8].ToString());
		Size = int.Parse(dataRow[9].ToString());
		Description = dataRow[10].ToString();
		Flip_90 = bool.Parse(dataRow[11].ToString());
		Flip_180 = bool.Parse(dataRow[12].ToString());
		Flip_270 = bool.Parse(dataRow[13].ToString());
		Flip_360 = bool.Parse(dataRow[14].ToString());
		Projectile = ulong.Parse(dataRow[15].ToString());
	}
}

public class UnitSummonDataTable_List : DataTableListBase
{
	public const string NAME = "UnitSummon";
	public const string DATAFILENAME = "UnitSummonData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			UnitSummonDataTable data = new UnitSummonDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class UnitSummonDataTable : DataTableBase
{
	public string Name;
	public byte BuyType;
	public int BuyCount;
	public ulong ChangeSerialNo;
	public int GroupID;
	public int Ratio;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		BuyType = byte.Parse(dataRow[2].ToString());
		BuyCount = int.Parse(dataRow[3].ToString());
		ChangeSerialNo = ulong.Parse(dataRow[4].ToString());
		GroupID = int.Parse(dataRow[5].ToString());
		Ratio = int.Parse(dataRow[6].ToString());
		Description = dataRow[7].ToString();
	}
}

public class WaveDataTable_List : DataTableListBase
{
	public const string NAME = "Wave";
	public const string DATAFILENAME = "WaveData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			WaveDataTable data = new WaveDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class WaveDataTable : DataTableBase
{
	public List<ulong> EnemySerialNo;
	public List<ushort> Level;
	public List<ushort> Count;
	public int WaveStartDelay;
	public ulong NextWave;
	public void Load(DataRow dataRow)
	{
		EnemySerialNo = new List<ulong>();
		string [] EnemySerialNo_tempArray = dataRow[1].ToString().Split(',');
		for( int i = 0; i < EnemySerialNo_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(EnemySerialNo_tempArray[i]);
			EnemySerialNo.Add(temp);
		}
		Level = new List<ushort>();
		string [] Level_tempArray = dataRow[2].ToString().Split(',');
		for( int i = 0; i < Level_tempArray.Length; ++i)
		{
			ushort temp = ushort.Parse(Level_tempArray[i]);
			Level.Add(temp);
		}
		Count = new List<ushort>();
		string [] Count_tempArray = dataRow[3].ToString().Split(',');
		for( int i = 0; i < Count_tempArray.Length; ++i)
		{
			ushort temp = ushort.Parse(Count_tempArray[i]);
			Count.Add(temp);
		}
		WaveStartDelay = int.Parse(dataRow[4].ToString());
		NextWave = ulong.Parse(dataRow[5].ToString());
	}
}

public class WorldDataTable_List : DataTableListBase
{
	public const string NAME = "World";
	public const string DATAFILENAME = "WorldData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			WorldDataTable data = new WorldDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class WorldDataTable : DataTableBase
{
	public string Name;
	public string BackGround1;
	public string BackGround2;
	public List<ulong> StageList;
	public ulong BossSerialNo;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		BackGround1 = dataRow[2].ToString();
		BackGround2 = dataRow[3].ToString();
		StageList = new List<ulong>();
		string [] StageList_tempArray = dataRow[4].ToString().Split(',');
		for( int i = 0; i < StageList_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(StageList_tempArray[i]);
			StageList.Add(temp);
		}
		BossSerialNo = ulong.Parse(dataRow[5].ToString());
		Description = dataRow[6].ToString();
	}
}


