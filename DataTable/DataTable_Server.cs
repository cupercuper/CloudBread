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

public class ActiveItemDataTable_List : DataTableListBase
{
	public const string NAME = "ActiveItem";
	public const string DATAFILENAME = "ActiveItemData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ActiveItemDataTable data = new ActiveItemDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ActiveItemDataTable : DataTableBase
{
	public byte Type;
	public string Name;
	public string Icon;
	public string Description;
	public string Value;
	public int Time;
	public byte Overlap;
	public byte Visible;
	public byte MoneyType;
	public int MoneyCount;
	public void Load(DataRow dataRow)
	{
		Type = byte.Parse(dataRow[1].ToString());
		Name = dataRow[2].ToString();
		Icon = dataRow[3].ToString();
		Description = dataRow[4].ToString();
		Value = dataRow[5].ToString();
		Time = int.Parse(dataRow[6].ToString());
		Overlap = byte.Parse(dataRow[7].ToString());
		Visible = byte.Parse(dataRow[8].ToString());
		MoneyType = byte.Parse(dataRow[9].ToString());
		MoneyCount = int.Parse(dataRow[10].ToString());
	}
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
	public int Level;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		HP = int.Parse(dataRow[3].ToString());
		Defence = int.Parse(dataRow[4].ToString());
		MoveSpeed = int.Parse(dataRow[5].ToString());
		Level = int.Parse(dataRow[6].ToString());
		Description = dataRow[7].ToString();
	}
}

public class BossDungeonDataTable_List : DataTableListBase
{
	public const string NAME = "BossDungeon";
	public const string DATAFILENAME = "BossDungeonData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			BossDungeonDataTable data = new BossDungeonDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class BossDungeonDataTable : DataTableBase
{
	public string Name;
	public string Icon;
	public string MapFolder;
	public string MapBackGround;
	public string MapPlace;
	public string MapShadow;
	public ulong BossSerialNo;
	public int BossLevel;
	public string BossIcon;
	public int Gold;
	public int EnhancementStone;
	public string Description;
	public int BossFailTime;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		MapFolder = dataRow[3].ToString();
		MapBackGround = dataRow[4].ToString();
		MapPlace = dataRow[5].ToString();
		MapShadow = dataRow[6].ToString();
		BossSerialNo = ulong.Parse(dataRow[7].ToString());
		BossLevel = int.Parse(dataRow[8].ToString());
		BossIcon = dataRow[9].ToString();
		Gold = int.Parse(dataRow[10].ToString());
		EnhancementStone = int.Parse(dataRow[11].ToString());
		Description = dataRow[12].ToString();
		BossFailTime = int.Parse(dataRow[13].ToString());
	}
}

public class BuffDataTable_List : DataTableListBase
{
	public const string NAME = "Buff";
	public const string DATAFILENAME = "BuffData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			BuffDataTable data = new BuffDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class BuffDataTable : DataTableBase
{
	public byte BuffType;
	public string Value;
	public string Description;
	public void Load(DataRow dataRow)
	{
		BuffType = byte.Parse(dataRow[1].ToString());
		Value = dataRow[2].ToString();
		Description = dataRow[3].ToString();
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
	public List<ulong> BuffList;
	public string Effect;
	public string Description;
	public string UITitle;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Type = byte.Parse(dataRow[2].ToString());
		BuffList = new List<ulong>();
		string [] BuffList_tempArray = dataRow[3].ToString().Split(',');
		for( int i = 0; i < BuffList_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(BuffList_tempArray[i]);
			BuffList.Add(temp);
		}
		Effect = dataRow[4].ToString();
		Description = dataRow[5].ToString();
		UITitle = dataRow[6].ToString();
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
	public string DamageEffect;
	public string DieEffect;
	public string Description;
	public byte GoldCoinCount;
	public int GoldValue;
	public byte GemCoinCount;
	public int GemValue;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		HP = int.Parse(dataRow[3].ToString());
		DefencePower = int.Parse(dataRow[4].ToString());
		MoveSpeed = int.Parse(dataRow[5].ToString());
		MoveType = byte.Parse(dataRow[6].ToString());
		DamageEffect = dataRow[7].ToString();
		DieEffect = dataRow[8].ToString();
		Description = dataRow[9].ToString();
		GoldCoinCount = byte.Parse(dataRow[10].ToString());
		GoldValue = int.Parse(dataRow[11].ToString());
		GemCoinCount = byte.Parse(dataRow[12].ToString());
		GemValue = int.Parse(dataRow[13].ToString());
	}
}

public class EnhancementDataTable_List : DataTableListBase
{
	public const string NAME = "Enhancement";
	public const string DATAFILENAME = "EnhancementData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			EnhancementDataTable data = new EnhancementDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class EnhancementDataTable : DataTableBase
{
	public int Probability_1;
	public int Probability_2;
	public int Probability_3;
	public int Probability_4;
	public int Probability_5;
	public int Grade_1;
	public int Grade_2;
	public int Grade_3;
	public int Grade_4;
	public int Grade_5;
	public int ProbabilityUp_Grade_1;
	public int ProbabilityUp_Grade_2;
	public int ProbabilityUp_Grade_3;
	public int ProbabilityUp_Grade_4;
	public int ProbabilityUp_Grade_5;
	public int FailSub_1;
	public int FailSub_2;
	public int FailSub_3;
	public int FailSub_4;
	public int FailSub_5;
	public void Load(DataRow dataRow)
	{
		Probability_1 = int.Parse(dataRow[1].ToString());
		Probability_2 = int.Parse(dataRow[2].ToString());
		Probability_3 = int.Parse(dataRow[3].ToString());
		Probability_4 = int.Parse(dataRow[4].ToString());
		Probability_5 = int.Parse(dataRow[5].ToString());
		Grade_1 = int.Parse(dataRow[6].ToString());
		Grade_2 = int.Parse(dataRow[7].ToString());
		Grade_3 = int.Parse(dataRow[8].ToString());
		Grade_4 = int.Parse(dataRow[9].ToString());
		Grade_5 = int.Parse(dataRow[10].ToString());
		ProbabilityUp_Grade_1 = int.Parse(dataRow[11].ToString());
		ProbabilityUp_Grade_2 = int.Parse(dataRow[12].ToString());
		ProbabilityUp_Grade_3 = int.Parse(dataRow[13].ToString());
		ProbabilityUp_Grade_4 = int.Parse(dataRow[14].ToString());
		ProbabilityUp_Grade_5 = int.Parse(dataRow[15].ToString());
		FailSub_1 = int.Parse(dataRow[16].ToString());
		FailSub_2 = int.Parse(dataRow[17].ToString());
		FailSub_3 = int.Parse(dataRow[18].ToString());
		FailSub_4 = int.Parse(dataRow[19].ToString());
		FailSub_5 = int.Parse(dataRow[20].ToString());
	}
}

public class GemBoxDataTable_List : DataTableListBase
{
	public const string NAME = "GemBox";
	public const string DATAFILENAME = "GemBoxData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			GemBoxDataTable data = new GemBoxDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class GemBoxDataTable : DataTableBase
{
	public int GemCount;
	public bool Advertising;
	public void Load(DataRow dataRow)
	{
		GemCount = int.Parse(dataRow[1].ToString());
		Advertising = bool.Parse(dataRow[2].ToString());
	}
}

public class GlobalSettingDataTable_List : DataTableListBase
{
	public const string NAME = "GlobalSetting";
	public const string DATAFILENAME = "GlobalSettingData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			GlobalSettingDataTable data = new GlobalSettingDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class GlobalSettingDataTable : DataTableBase
{
	public int UnitListChangeTime;
	public int UnitListChangeGem;
	public int UnitStoreActiveGem;
	public int GemUseAddProbability;
	public float CoinGetTime;
	public int GemBoxProbability;
	public int GemBoxCount;
	public int GemProbability;
	public int GemCount;
	public int GemBoxDelay;
	public byte BossDugeonTicketCount;
	public byte BossDugeonAddMoney;
	public int CaptainChangeGoldRate;
	public int CaptainChangeGoldMaxRate;
	public int CaptainChangeGoldAuto;
	public void Load(DataRow dataRow)
	{
		UnitListChangeTime = int.Parse(dataRow[1].ToString());
		UnitListChangeGem = int.Parse(dataRow[2].ToString());
		UnitStoreActiveGem = int.Parse(dataRow[3].ToString());
		GemUseAddProbability = int.Parse(dataRow[4].ToString());
		CoinGetTime = float.Parse(dataRow[5].ToString());
		GemBoxProbability = int.Parse(dataRow[6].ToString());
		GemBoxCount = int.Parse(dataRow[7].ToString());
		GemProbability = int.Parse(dataRow[8].ToString());
		GemCount = int.Parse(dataRow[9].ToString());
		GemBoxDelay = int.Parse(dataRow[10].ToString());
		BossDugeonTicketCount = byte.Parse(dataRow[11].ToString());
		BossDugeonAddMoney = byte.Parse(dataRow[12].ToString());
		CaptainChangeGoldRate = int.Parse(dataRow[13].ToString());
		CaptainChangeGoldMaxRate = int.Parse(dataRow[14].ToString());
		CaptainChangeGoldAuto = int.Parse(dataRow[15].ToString());
	}
}

public class ItemDataTable_List : DataTableListBase
{
	public const string NAME = "Item";
	public const string DATAFILENAME = "ItemData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ItemDataTable data = new ItemDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ItemDataTable : DataTableBase
{
	public string Name;
	public byte ChangeType;
	public string Value;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		ChangeType = byte.Parse(dataRow[2].ToString());
		Value = dataRow[3].ToString();
		Description = dataRow[4].ToString();
	}
}

public class LevelUpDataTable_List : DataTableListBase
{
	public const string NAME = "LevelUp";
	public const string DATAFILENAME = "LevelUpData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			LevelUpDataTable data = new LevelUpDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class LevelUpDataTable : DataTableBase
{
	public int LevelUpGold;
	public void Load(DataRow dataRow)
	{
		LevelUpGold = int.Parse(dataRow[1].ToString());
	}
}

public class ModeDataTable_List : DataTableListBase
{
	public const string NAME = "Mode";
	public const string DATAFILENAME = "ModeData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ModeDataTable data = new ModeDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ModeDataTable : DataTableBase
{
	public string Name;
	public string Icon;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
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

public class ShopDataTable_List : DataTableListBase
{
	public const string NAME = "Shop";
	public const string DATAFILENAME = "ShopData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ShopDataTable data = new ShopDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ShopDataTable : DataTableBase
{
	public string Name;
	public string Icon;
	public byte Type;
	public byte Limit;
	public byte MoneyType;
	public int MoneyCount;
	public List<ulong> ItemList;
	public string ProductId;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		Type = byte.Parse(dataRow[3].ToString());
		Limit = byte.Parse(dataRow[4].ToString());
		MoneyType = byte.Parse(dataRow[5].ToString());
		MoneyCount = int.Parse(dataRow[6].ToString());
		ItemList = new List<ulong>();
		string [] ItemList_tempArray = dataRow[7].ToString().Split(',');
		for( int i = 0; i < ItemList_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(ItemList_tempArray[i]);
			ItemList.Add(temp);
		}
		ProductId = dataRow[8].ToString();
		Description = dataRow[9].ToString();
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

public class ToolTipDataTable_List : DataTableListBase
{
	public const string NAME = "ToolTip";
	public const string DATAFILENAME = "ToolTipData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ToolTipDataTable data = new ToolTipDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ToolTipDataTable : DataTableBase
{
	public string ToolTip;
	public void Load(DataRow dataRow)
	{
		ToolTip = dataRow[1].ToString();
	}
}

public class TutorialDataTable_List : DataTableListBase
{
	public const string NAME = "Tutorial";
	public const string DATAFILENAME = "TutorialData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			TutorialDataTable data = new TutorialDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class TutorialDataTable : DataTableBase
{
	public List<string> TutorialList;
	public void Load(DataRow dataRow)
	{
		TutorialList = new List<string>();
		string [] TutorialList_tempArray = dataRow[1].ToString().Split(',');
		for( int i = 0; i < TutorialList_tempArray.Length; ++i)
		{
			string temp = TutorialList_tempArray[i];
			TutorialList.Add(temp);
		}
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
	public string Icon;
	public byte Type;
	public byte Grade;
	public int AttackCoolTime;
	public int AttackRange;
	public int AttackPowerG;
	public int AttackPowerF;
	public int CriticalRate;
	public int SplashRangeG;
	public int SplashRangeF;
	public int Size;
	public string Description;
	public ulong ProjectileG;
	public ulong ProjectileF;
	public List<byte> AttackType;
	public int UnitStoreMoney;
	public string FireEffectG;
	public string FireEffectF;
	public string TargetPosEffect;
	public byte AttackDirectionType;
	public string FireSoundG;
	public string FireSoundF;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		Icon = dataRow[3].ToString();
		Type = byte.Parse(dataRow[4].ToString());
		Grade = byte.Parse(dataRow[5].ToString());
		AttackCoolTime = int.Parse(dataRow[6].ToString());
		AttackRange = int.Parse(dataRow[7].ToString());
		AttackPowerG = int.Parse(dataRow[8].ToString());
		AttackPowerF = int.Parse(dataRow[9].ToString());
		CriticalRate = int.Parse(dataRow[10].ToString());
		SplashRangeG = int.Parse(dataRow[11].ToString());
		SplashRangeF = int.Parse(dataRow[12].ToString());
		Size = int.Parse(dataRow[13].ToString());
		Description = dataRow[14].ToString();
		ProjectileG = ulong.Parse(dataRow[15].ToString());
		ProjectileF = ulong.Parse(dataRow[16].ToString());
		AttackType = new List<byte>();
		string [] AttackType_tempArray = dataRow[17].ToString().Split(',');
		for( int i = 0; i < AttackType_tempArray.Length; ++i)
		{
			byte temp = byte.Parse(AttackType_tempArray[i]);
			AttackType.Add(temp);
		}
		UnitStoreMoney = int.Parse(dataRow[18].ToString());
		FireEffectG = dataRow[19].ToString();
		FireEffectF = dataRow[20].ToString();
		TargetPosEffect = dataRow[21].ToString();
		AttackDirectionType = byte.Parse(dataRow[22].ToString());
		FireSoundG = dataRow[23].ToString();
		FireSoundF = dataRow[24].ToString();
	}
}

public class UnitSlotDataTable_List : DataTableListBase
{
	public const string NAME = "UnitSlot";
	public const string DATAFILENAME = "UnitSlotData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			UnitSlotDataTable data = new UnitSlotDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class UnitSlotDataTable : DataTableBase
{
	public byte UnitMaxCount;
	public int UpgradeMoney;
	public void Load(DataRow dataRow)
	{
		UnitMaxCount = byte.Parse(dataRow[1].ToString());
		UpgradeMoney = int.Parse(dataRow[2].ToString());
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

public class UnitSummonRandomTicketDataTable_List : DataTableListBase
{
	public const string NAME = "UnitSummonRandomTicket";
	public const string DATAFILENAME = "UnitSummonRandomTicketData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			UnitSummonRandomTicketDataTable data = new UnitSummonRandomTicketDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class UnitSummonRandomTicketDataTable : DataTableBase
{
	public string Name;
	public int GroupID;
	public string Description;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		GroupID = int.Parse(dataRow[2].ToString());
		Description = dataRow[3].ToString();
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
	public string Icon;
	public string MapFolder;
	public string MapBackGround;
	public string MapPlace;
	public string MapShadow;
	public List<ulong> StageList;
	public ulong BossSerialNo;
	public int BossLevel;
	public int EnhancementStone;
	public string Description;
	public int BossFailTime;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		MapFolder = dataRow[3].ToString();
		MapBackGround = dataRow[4].ToString();
		MapPlace = dataRow[5].ToString();
		MapShadow = dataRow[6].ToString();
		StageList = new List<ulong>();
		string [] StageList_tempArray = dataRow[7].ToString().Split(',');
		for( int i = 0; i < StageList_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(StageList_tempArray[i]);
			StageList.Add(temp);
		}
		BossSerialNo = ulong.Parse(dataRow[8].ToString());
		BossLevel = int.Parse(dataRow[9].ToString());
		EnhancementStone = int.Parse(dataRow[10].ToString());
		Description = dataRow[11].ToString();
		BossFailTime = int.Parse(dataRow[12].ToString());
	}
}


