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

public class AccAttendanceRewardDataTable_List : DataTableListBase
{
	public const string NAME = "AccAttendanceReward";
	public const string DATAFILENAME = "AccAttendanceRewardData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			AccAttendanceRewardDataTable data = new AccAttendanceRewardDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class AccAttendanceRewardDataTable : DataTableBase
{
	public byte Table_ItemType_1;
	public byte Table_ItemSubType_1;
	public string Table_ItemValue_1;
	public byte Table_ItemType_2;
	public byte Table_ItemSubType_2;
	public string Table_ItemValue_2;
	public byte Table_ItemType_3;
	public byte Table_ItemSubType_3;
	public string Table_ItemValue_3;
	public void Load(DataRow dataRow)
	{
		Table_ItemType_1 = byte.Parse(dataRow[1].ToString());
		Table_ItemSubType_1 = byte.Parse(dataRow[2].ToString());
		Table_ItemValue_1 = dataRow[3].ToString();
		Table_ItemType_2 = byte.Parse(dataRow[4].ToString());
		Table_ItemSubType_2 = byte.Parse(dataRow[5].ToString());
		Table_ItemValue_2 = dataRow[6].ToString();
		Table_ItemType_3 = byte.Parse(dataRow[7].ToString());
		Table_ItemSubType_3 = byte.Parse(dataRow[8].ToString());
		Table_ItemValue_3 = dataRow[9].ToString();
	}
}

public class AchievementDataTable_List : DataTableListBase
{
	public const string NAME = "Achievement";
	public const string DATAFILENAME = "AchievementData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			AchievementDataTable data = new AchievementDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class AchievementDataTable : DataTableBase
{
	public string Title;
	public string Icon;
	public byte ItemType;
	public byte ItemSubType;
	public string ItemValue;
	public byte AchievementType;
	public byte AchievementSubType;
	public string AchievementValue;
	public ulong NextAchievement;
	public void Load(DataRow dataRow)
	{
		Title = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		ItemType = byte.Parse(dataRow[3].ToString());
		ItemSubType = byte.Parse(dataRow[4].ToString());
		ItemValue = dataRow[5].ToString();
		AchievementType = byte.Parse(dataRow[6].ToString());
		AchievementSubType = byte.Parse(dataRow[7].ToString());
		AchievementValue = dataRow[8].ToString();
		NextAchievement = ulong.Parse(dataRow[9].ToString());
	}
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

public class BaseCampDataTable_List : DataTableListBase
{
	public const string NAME = "BaseCamp";
	public const string DATAFILENAME = "BaseCampData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			BaseCampDataTable data = new BaseCampDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class BaseCampDataTable : DataTableBase
{
	public string Name;
	public string Description;
	public byte TabType;
	public ulong BuffType;
	public int BuffValue;
	public int UpgradeValueRatio;
	public ushort MaxLevel;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Description = dataRow[2].ToString();
		TabType = byte.Parse(dataRow[3].ToString());
		BuffType = ulong.Parse(dataRow[4].ToString());
		BuffValue = int.Parse(dataRow[5].ToString());
		UpgradeValueRatio = int.Parse(dataRow[6].ToString());
		MaxLevel = ushort.Parse(dataRow[7].ToString());
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

public class BoxDataTable_List : DataTableListBase
{
	public const string NAME = "Box";
	public const string DATAFILENAME = "BoxData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			BoxDataTable data = new BoxDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class BoxDataTable : DataTableBase
{
	public byte BoxType;
	public byte ItemType;
	public byte ItemSubType;
	public string ItemValue;
	public uint Ratio;
	public void Load(DataRow dataRow)
	{
		BoxType = byte.Parse(dataRow[1].ToString());
		ItemType = byte.Parse(dataRow[2].ToString());
		ItemSubType = byte.Parse(dataRow[3].ToString());
		ItemValue = dataRow[4].ToString();
		Ratio = uint.Parse(dataRow[5].ToString());
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
	public byte TargetType1;
	public byte TargetType2;
	public string Title;
	public string Icon;
	public string SubIcon;
	public void Load(DataRow dataRow)
	{
		BuffType = byte.Parse(dataRow[1].ToString());
		TargetType1 = byte.Parse(dataRow[2].ToString());
		TargetType2 = byte.Parse(dataRow[3].ToString());
		Title = dataRow[4].ToString();
		Icon = dataRow[5].ToString();
		SubIcon = dataRow[6].ToString();
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
	public string Icon;
	public byte Type;
	public string Ability;
	public string Description;
	public ulong BuffType;
	public int BuffValue;
	public int BuffRatio;
	public List<ulong> LevelUpBuffList;
	public double LevelUpFirstMoney;
	public int LevelUpMoneyRate;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		Type = byte.Parse(dataRow[3].ToString());
		Ability = dataRow[4].ToString();
		Description = dataRow[5].ToString();
		BuffType = ulong.Parse(dataRow[6].ToString());
		BuffValue = int.Parse(dataRow[7].ToString());
		BuffRatio = int.Parse(dataRow[8].ToString());
		LevelUpBuffList = new List<ulong>();
		string [] LevelUpBuffList_tempArray = dataRow[9].ToString().Split(',');
		for( int i = 0; i < LevelUpBuffList_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(LevelUpBuffList_tempArray[i]);
			LevelUpBuffList.Add(temp);
		}
		LevelUpFirstMoney = double.Parse(dataRow[10].ToString());
		LevelUpMoneyRate = int.Parse(dataRow[11].ToString());
	}
}

public class ContinueAttendanceRewardDataTable_List : DataTableListBase
{
	public const string NAME = "ContinueAttendanceReward";
	public const string DATAFILENAME = "ContinueAttendanceRewardData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ContinueAttendanceRewardDataTable data = new ContinueAttendanceRewardDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ContinueAttendanceRewardDataTable : DataTableBase
{
	public byte Table_ItemType_1;
	public byte Table_ItemSubType_1;
	public string Table_ItemValue_1;
	public byte Table_ItemType_2;
	public byte Table_ItemSubType_2;
	public string Table_ItemValue_2;
	public byte Table_ItemType_3;
	public byte Table_ItemSubType_3;
	public string Table_ItemValue_3;
	public void Load(DataRow dataRow)
	{
		Table_ItemType_1 = byte.Parse(dataRow[1].ToString());
		Table_ItemSubType_1 = byte.Parse(dataRow[2].ToString());
		Table_ItemValue_1 = dataRow[3].ToString();
		Table_ItemType_2 = byte.Parse(dataRow[4].ToString());
		Table_ItemSubType_2 = byte.Parse(dataRow[5].ToString());
		Table_ItemValue_2 = dataRow[6].ToString();
		Table_ItemType_3 = byte.Parse(dataRow[7].ToString());
		Table_ItemSubType_3 = byte.Parse(dataRow[8].ToString());
		Table_ItemValue_3 = dataRow[9].ToString();
	}
}

public class DailyQuestDataTable_List : DataTableListBase
{
	public const string NAME = "DailyQuest";
	public const string DATAFILENAME = "DailyQuestData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			DailyQuestDataTable data = new DailyQuestDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class DailyQuestDataTable : DataTableBase
{
	public string Title;
	public string Description;
	public string Icon;
	public byte Grade;
	public byte ItemType;
	public byte ItemSubType;
	public string ItemValue;
	public byte QuestType;
	public byte QuestSubType;
	public string QuestValue;
	public void Load(DataRow dataRow)
	{
		Title = dataRow[1].ToString();
		Description = dataRow[2].ToString();
		Icon = dataRow[3].ToString();
		Grade = byte.Parse(dataRow[4].ToString());
		ItemType = byte.Parse(dataRow[5].ToString());
		ItemSubType = byte.Parse(dataRow[6].ToString());
		ItemValue = dataRow[7].ToString();
		QuestType = byte.Parse(dataRow[8].ToString());
		QuestSubType = byte.Parse(dataRow[9].ToString());
		QuestValue = dataRow[10].ToString();
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
	public byte DefenceType;
	public double HP;
	public int DefencePower;
	public int MoveSpeed;
	public byte MoveType;
	public string DamageEffect;
	public string DieEffect;
	public string Description;
	public byte GoldCoinCount;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		DefenceType = byte.Parse(dataRow[3].ToString());
		HP = double.Parse(dataRow[4].ToString());
		DefencePower = int.Parse(dataRow[5].ToString());
		MoveSpeed = int.Parse(dataRow[6].ToString());
		MoveType = byte.Parse(dataRow[7].ToString());
		DamageEffect = dataRow[8].ToString();
		DieEffect = dataRow[9].ToString();
		Description = dataRow[10].ToString();
		GoldCoinCount = byte.Parse(dataRow[11].ToString());
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
	public int StoneCnt;
	public ulong AccStoneCnt;
	public int Value;
	public void Load(DataRow dataRow)
	{
		StoneCnt = int.Parse(dataRow[1].ToString());
		AccStoneCnt = ulong.Parse(dataRow[2].ToString());
		Value = int.Parse(dataRow[3].ToString());
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
	public float CoinGetTime;
	public byte BossDugeonTicketCount;
	public byte BossDugeonAddMoney;
	public int DailyQuestResetTIme;
	public int LuckySupplyShipResetTime;
	public ulong ReturnStage;
	public int ScienceDroneDestoryTime;
	public int ScienceDroneCreateTime;
	public int GameSpeedItemTime;
	public byte GameSpeedItemMaxCount;
	public int MonsterMineralValue;
	public ulong WarpMonsterID;
	public void Load(DataRow dataRow)
	{
		CoinGetTime = float.Parse(dataRow[1].ToString());
		BossDugeonTicketCount = byte.Parse(dataRow[2].ToString());
		BossDugeonAddMoney = byte.Parse(dataRow[3].ToString());
		DailyQuestResetTIme = int.Parse(dataRow[4].ToString());
		LuckySupplyShipResetTime = int.Parse(dataRow[5].ToString());
		ReturnStage = ulong.Parse(dataRow[6].ToString());
		ScienceDroneDestoryTime = int.Parse(dataRow[7].ToString());
		ScienceDroneCreateTime = int.Parse(dataRow[8].ToString());
		GameSpeedItemTime = int.Parse(dataRow[9].ToString());
		GameSpeedItemMaxCount = byte.Parse(dataRow[10].ToString());
		MonsterMineralValue = int.Parse(dataRow[11].ToString());
		WarpMonsterID = ulong.Parse(dataRow[12].ToString());
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

public class LevelUpSkillDataTable_List : DataTableListBase
{
	public const string NAME = "LevelUpSkill";
	public const string DATAFILENAME = "LevelUpSkillData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			LevelUpSkillDataTable data = new LevelUpSkillDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class LevelUpSkillDataTable : DataTableBase
{
	public string Title;
	public string Icon;
	public int OpenLevel;
	public ulong BuffType;
	public int BuffValue;
	public void Load(DataRow dataRow)
	{
		Title = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		OpenLevel = int.Parse(dataRow[3].ToString());
		BuffType = ulong.Parse(dataRow[4].ToString());
		BuffValue = int.Parse(dataRow[5].ToString());
	}
}

public class LuckySupplyShipDataTable_List : DataTableListBase
{
	public const string NAME = "LuckySupplyShip";
	public const string DATAFILENAME = "LuckySupplyShipData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			LuckySupplyShipDataTable data = new LuckySupplyShipDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class LuckySupplyShipDataTable : DataTableBase
{
	public List<int> ItemTypeList;
	public List<byte> ItemSubTypeList;
	public List<string> ItemValueLIst;
	public List<byte> ItemRateList;
	public byte JackPok;
	public short GemCount;
	public void Load(DataRow dataRow)
	{
		ItemTypeList = new List<int>();
		string [] ItemTypeList_tempArray = dataRow[1].ToString().Split(',');
		for( int i = 0; i < ItemTypeList_tempArray.Length; ++i)
		{
			int temp = int.Parse(ItemTypeList_tempArray[i]);
			ItemTypeList.Add(temp);
		}
		ItemSubTypeList = new List<byte>();
		string [] ItemSubTypeList_tempArray = dataRow[2].ToString().Split(',');
		for( int i = 0; i < ItemSubTypeList_tempArray.Length; ++i)
		{
			byte temp = byte.Parse(ItemSubTypeList_tempArray[i]);
			ItemSubTypeList.Add(temp);
		}
		ItemValueLIst = new List<string>();
		string [] ItemValueLIst_tempArray = dataRow[3].ToString().Split(',');
		for( int i = 0; i < ItemValueLIst_tempArray.Length; ++i)
		{
			string temp = ItemValueLIst_tempArray[i];
			ItemValueLIst.Add(temp);
		}
		ItemRateList = new List<byte>();
		string [] ItemRateList_tempArray = dataRow[4].ToString().Split(',');
		for( int i = 0; i < ItemRateList_tempArray.Length; ++i)
		{
			byte temp = byte.Parse(ItemRateList_tempArray[i]);
			ItemRateList.Add(temp);
		}
		JackPok = byte.Parse(dataRow[5].ToString());
		GemCount = short.Parse(dataRow[6].ToString());
	}
}

public class MineralBoxDataTable_List : DataTableListBase
{
	public const string NAME = "MineralBox";
	public const string DATAFILENAME = "MineralBoxData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			MineralBoxDataTable data = new MineralBoxDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class MineralBoxDataTable : DataTableBase
{
	public byte BoxType;
	public int Value;
	public ulong DefaultHPNo;
	public void Load(DataRow dataRow)
	{
		BoxType = byte.Parse(dataRow[1].ToString());
		Value = int.Parse(dataRow[2].ToString());
		DefaultHPNo = ulong.Parse(dataRow[3].ToString());
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

public class RelicDataTable_List : DataTableListBase
{
	public const string NAME = "Relic";
	public const string DATAFILENAME = "RelicData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			RelicDataTable data = new RelicDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class RelicDataTable : DataTableBase
{
	public string Name;
	public string Icon;
	public byte Grade;
	public ulong UpgradeTableNo;
	public ulong Buff_1;
	public int BuffMinValue_1;
	public int BuffMaxValue_1;
	public int BuffLevelRatio_1;
	public ulong Buff_2;
	public int BuffMinValue_2;
	public int BuffMaxValue_2;
	public int BuffLevelRatio_2;
	public ulong Buff_3;
	public int BuffMinValue_3;
	public int BuffMaxValue_3;
	public int BuffLevelRatio_3;
	public int SummonRatio;
	public ushort MaxLevel;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		Grade = byte.Parse(dataRow[3].ToString());
		UpgradeTableNo = ulong.Parse(dataRow[4].ToString());
		Buff_1 = ulong.Parse(dataRow[5].ToString());
		BuffMinValue_1 = int.Parse(dataRow[6].ToString());
		BuffMaxValue_1 = int.Parse(dataRow[7].ToString());
		BuffLevelRatio_1 = int.Parse(dataRow[8].ToString());
		Buff_2 = ulong.Parse(dataRow[9].ToString());
		BuffMinValue_2 = int.Parse(dataRow[10].ToString());
		BuffMaxValue_2 = int.Parse(dataRow[11].ToString());
		BuffLevelRatio_2 = int.Parse(dataRow[12].ToString());
		Buff_3 = ulong.Parse(dataRow[13].ToString());
		BuffMinValue_3 = int.Parse(dataRow[14].ToString());
		BuffMaxValue_3 = int.Parse(dataRow[15].ToString());
		BuffLevelRatio_3 = int.Parse(dataRow[16].ToString());
		SummonRatio = int.Parse(dataRow[17].ToString());
		MaxLevel = ushort.Parse(dataRow[18].ToString());
	}
}

public class RelicDestroyDataTable_List : DataTableListBase
{
	public const string NAME = "RelicDestroy";
	public const string DATAFILENAME = "RelicDestroyData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			RelicDestroyDataTable data = new RelicDestroyDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class RelicDestroyDataTable : DataTableBase
{
	public ushort DestroyMoney;
	public void Load(DataRow dataRow)
	{
		DestroyMoney = ushort.Parse(dataRow[1].ToString());
	}
}

public class RelicInventorySlotDataTable_List : DataTableListBase
{
	public const string NAME = "RelicInventorySlot";
	public const string DATAFILENAME = "RelicInventorySlotData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			RelicInventorySlotDataTable data = new RelicInventorySlotDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class RelicInventorySlotDataTable : DataTableBase
{
	public ushort Count;
	public ushort UpgradeMoney;
	public void Load(DataRow dataRow)
	{
		Count = ushort.Parse(dataRow[1].ToString());
		UpgradeMoney = ushort.Parse(dataRow[2].ToString());
	}
}

public class RelicSlotDataTable_List : DataTableListBase
{
	public const string NAME = "RelicSlot";
	public const string DATAFILENAME = "RelicSlotData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			RelicSlotDataTable data = new RelicSlotDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class RelicSlotDataTable : DataTableBase
{
	public ushort Count;
	public ushort UpgradeMoney;
	public void Load(DataRow dataRow)
	{
		Count = ushort.Parse(dataRow[1].ToString());
		UpgradeMoney = ushort.Parse(dataRow[2].ToString());
	}
}

public class RelicUpgradeDataTable_List : DataTableListBase
{
	public const string NAME = "RelicUpgrade";
	public const string DATAFILENAME = "RelicUpgradeData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			RelicUpgradeDataTable data = new RelicUpgradeDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class RelicUpgradeDataTable : DataTableBase
{
	public string Name;
	public double UpgradeFirstMoney;
	public int UpgradeMoneyRatio;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		UpgradeFirstMoney = double.Parse(dataRow[2].ToString());
		UpgradeMoneyRatio = int.Parse(dataRow[3].ToString());
	}
}

public class ResourceDrillDataTable_List : DataTableListBase
{
	public const string NAME = "ResourceDrill";
	public const string DATAFILENAME = "ResourceDrillData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ResourceDrillDataTable data = new ResourceDrillDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ResourceDrillDataTable : DataTableBase
{
	public byte ItemType;
	public byte ItemSubType;
	public string ItemValue;
	public int ResetTime;
	public string Description;
	public void Load(DataRow dataRow)
	{
		ItemType = byte.Parse(dataRow[1].ToString());
		ItemSubType = byte.Parse(dataRow[2].ToString());
		ItemValue = dataRow[3].ToString();
		ResetTime = int.Parse(dataRow[4].ToString());
		Description = dataRow[5].ToString();
	}
}

public class RTDModeDataTable_List : DataTableListBase
{
	public const string NAME = "RTDMode";
	public const string DATAFILENAME = "RTDModeData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			RTDModeDataTable data = new RTDModeDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class RTDModeDataTable : DataTableBase
{
	public string Name;
	public byte HP;
	public int StartDelayTime;
	public int RoundDelayTime;
	public int InitSummon;
	public int MaxSummon;
	public int RoundSummon;
	public int RoundUpgrade;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		HP = byte.Parse(dataRow[2].ToString());
		StartDelayTime = int.Parse(dataRow[3].ToString());
		RoundDelayTime = int.Parse(dataRow[4].ToString());
		InitSummon = int.Parse(dataRow[5].ToString());
		MaxSummon = int.Parse(dataRow[6].ToString());
		RoundSummon = int.Parse(dataRow[7].ToString());
		RoundUpgrade = int.Parse(dataRow[8].ToString());
	}
}

public class RTDModeRoundDataTable_List : DataTableListBase
{
	public const string NAME = "RTDModeRound";
	public const string DATAFILENAME = "RTDModeRoundData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			RTDModeRoundDataTable data = new RTDModeRoundDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class RTDModeRoundDataTable : DataTableBase
{
	public List<ulong> EnemyList;
	public List<int> HPList;
	public List<int> DefencePowerLIst;
	public List<ushort> CountList;
	public byte BossStage;
	public int CoolTime;
	public void Load(DataRow dataRow)
	{
		EnemyList = new List<ulong>();
		string [] EnemyList_tempArray = dataRow[1].ToString().Split(',');
		for( int i = 0; i < EnemyList_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(EnemyList_tempArray[i]);
			EnemyList.Add(temp);
		}
		HPList = new List<int>();
		string [] HPList_tempArray = dataRow[2].ToString().Split(',');
		for( int i = 0; i < HPList_tempArray.Length; ++i)
		{
			int temp = int.Parse(HPList_tempArray[i]);
			HPList.Add(temp);
		}
		DefencePowerLIst = new List<int>();
		string [] DefencePowerLIst_tempArray = dataRow[3].ToString().Split(',');
		for( int i = 0; i < DefencePowerLIst_tempArray.Length; ++i)
		{
			int temp = int.Parse(DefencePowerLIst_tempArray[i]);
			DefencePowerLIst.Add(temp);
		}
		CountList = new List<ushort>();
		string [] CountList_tempArray = dataRow[4].ToString().Split(',');
		for( int i = 0; i < CountList_tempArray.Length; ++i)
		{
			ushort temp = ushort.Parse(CountList_tempArray[i]);
			CountList.Add(temp);
		}
		BossStage = byte.Parse(dataRow[5].ToString());
		CoolTime = int.Parse(dataRow[6].ToString());
	}
}

public class ScienceDroneDataTable_List : DataTableListBase
{
	public const string NAME = "ScienceDrone";
	public const string DATAFILENAME = "ScienceDroneData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			ScienceDroneDataTable data = new ScienceDroneDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class ScienceDroneDataTable : DataTableBase
{
	public byte ItemType;
	public byte ItemSubType;
	public string ItemValue;
	public bool Advertising;
	public int Ratio;
	public void Load(DataRow dataRow)
	{
		ItemType = byte.Parse(dataRow[1].ToString());
		ItemSubType = byte.Parse(dataRow[2].ToString());
		ItemValue = dataRow[3].ToString();
		Advertising = bool.Parse(dataRow[4].ToString());
		Ratio = int.Parse(dataRow[5].ToString());
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
	public byte Visible;
	public string Description;
	public byte Type;
	public byte Limit;
	public byte MoneyType;
	public int MoneyCount;
	public List<byte> ItemTypeList;
	public List<byte> ItemSubTypeList;
	public List<string> ItemValueList;
	public string ProductId;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Icon = dataRow[2].ToString();
		Visible = byte.Parse(dataRow[3].ToString());
		Description = dataRow[4].ToString();
		Type = byte.Parse(dataRow[5].ToString());
		Limit = byte.Parse(dataRow[6].ToString());
		MoneyType = byte.Parse(dataRow[7].ToString());
		MoneyCount = int.Parse(dataRow[8].ToString());
		ItemTypeList = new List<byte>();
		string [] ItemTypeList_tempArray = dataRow[9].ToString().Split(',');
		for( int i = 0; i < ItemTypeList_tempArray.Length; ++i)
		{
			byte temp = byte.Parse(ItemTypeList_tempArray[i]);
			ItemTypeList.Add(temp);
		}
		ItemSubTypeList = new List<byte>();
		string [] ItemSubTypeList_tempArray = dataRow[10].ToString().Split(',');
		for( int i = 0; i < ItemSubTypeList_tempArray.Length; ++i)
		{
			byte temp = byte.Parse(ItemSubTypeList_tempArray[i]);
			ItemSubTypeList.Add(temp);
		}
		ItemValueList = new List<string>();
		string [] ItemValueList_tempArray = dataRow[11].ToString().Split(',');
		for( int i = 0; i < ItemValueList_tempArray.Length; ++i)
		{
			string temp = ItemValueList_tempArray[i];
			ItemValueList.Add(temp);
		}
		ProductId = dataRow[12].ToString();
	}
}

public class SkillItemDataTable_List : DataTableListBase
{
	public const string NAME = "SkillItem";
	public const string DATAFILENAME = "SkillItemData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			SkillItemDataTable data = new SkillItemDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class SkillItemDataTable : DataTableBase
{
	public byte Type;
	public string Icon;
	public string Title;
	public string Description;
	public string ItemValue;
	public int Duration;
	public string EffectName;
	public string TargetEffectName;
	public int Range;
	public int CoolTime;
	public int BuffApplyTime;
	public ulong OpenStage;
	public void Load(DataRow dataRow)
	{
		Type = byte.Parse(dataRow[1].ToString());
		Icon = dataRow[2].ToString();
		Title = dataRow[3].ToString();
		Description = dataRow[4].ToString();
		ItemValue = dataRow[5].ToString();
		Duration = int.Parse(dataRow[6].ToString());
		EffectName = dataRow[7].ToString();
		TargetEffectName = dataRow[8].ToString();
		Range = int.Parse(dataRow[9].ToString());
		CoolTime = int.Parse(dataRow[10].ToString());
		BuffApplyTime = int.Parse(dataRow[11].ToString());
		OpenStage = ulong.Parse(dataRow[12].ToString());
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

public class TypeOfTypeDataTable_List : DataTableListBase
{
	public const string NAME = "TypeOfType";
	public const string DATAFILENAME = "TypeOfTypeData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			TypeOfTypeDataTable data = new TypeOfTypeDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class TypeOfTypeDataTable : DataTableBase
{
	public int Flame;
	public int Rifle;
	public int Machine;
	public int Cannon;
	public int Missile;
	public void Load(DataRow dataRow)
	{
		Flame = int.Parse(dataRow[1].ToString());
		Rifle = int.Parse(dataRow[2].ToString());
		Machine = int.Parse(dataRow[3].ToString());
		Cannon = int.Parse(dataRow[4].ToString());
		Missile = int.Parse(dataRow[5].ToString());
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
	public byte AttackTypeG;
	public byte AttackTypeF;
	public byte DefenceType;
	public double HP;
	public int Defence;
	public int AttackCoolTime;
	public int MaxAttackRange;
	public int MinAttackRange;
	public double AttackPowerG;
	public double AttackPowerF;
	public int CriticalRate;
	public int SplashRangeG;
	public int SplashRangeF;
	public int Size;
	public string Description;
	public ulong ProjectileG;
	public ulong ProjectileF;
	public List<byte> AttackTargetType;
	public string FireEffectG;
	public string FireEffectF;
	public string TargetPosEffect;
	public byte AttackDirectionType;
	public string FireSoundG;
	public string FireSoundF;
	public byte MovingFire;
	public double LevelUpFirstMoney;
	public int LevelUpMoneyRate;
	public double OpenMoney;
	public List<ulong> LevelUpSkillTable;
	public ulong OpenStage;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		PrefabName = dataRow[2].ToString();
		Icon = dataRow[3].ToString();
		Type = byte.Parse(dataRow[4].ToString());
		AttackTypeG = byte.Parse(dataRow[5].ToString());
		AttackTypeF = byte.Parse(dataRow[6].ToString());
		DefenceType = byte.Parse(dataRow[7].ToString());
		HP = double.Parse(dataRow[8].ToString());
		Defence = int.Parse(dataRow[9].ToString());
		AttackCoolTime = int.Parse(dataRow[10].ToString());
		MaxAttackRange = int.Parse(dataRow[11].ToString());
		MinAttackRange = int.Parse(dataRow[12].ToString());
		AttackPowerG = double.Parse(dataRow[13].ToString());
		AttackPowerF = double.Parse(dataRow[14].ToString());
		CriticalRate = int.Parse(dataRow[15].ToString());
		SplashRangeG = int.Parse(dataRow[16].ToString());
		SplashRangeF = int.Parse(dataRow[17].ToString());
		Size = int.Parse(dataRow[18].ToString());
		Description = dataRow[19].ToString();
		ProjectileG = ulong.Parse(dataRow[20].ToString());
		ProjectileF = ulong.Parse(dataRow[21].ToString());
		AttackTargetType = new List<byte>();
		string [] AttackTargetType_tempArray = dataRow[22].ToString().Split(',');
		for( int i = 0; i < AttackTargetType_tempArray.Length; ++i)
		{
			byte temp = byte.Parse(AttackTargetType_tempArray[i]);
			AttackTargetType.Add(temp);
		}
		FireEffectG = dataRow[23].ToString();
		FireEffectF = dataRow[24].ToString();
		TargetPosEffect = dataRow[25].ToString();
		AttackDirectionType = byte.Parse(dataRow[26].ToString());
		FireSoundG = dataRow[27].ToString();
		FireSoundF = dataRow[28].ToString();
		MovingFire = byte.Parse(dataRow[29].ToString());
		LevelUpFirstMoney = double.Parse(dataRow[30].ToString());
		LevelUpMoneyRate = int.Parse(dataRow[31].ToString());
		OpenMoney = double.Parse(dataRow[32].ToString());
		LevelUpSkillTable = new List<ulong>();
		string [] LevelUpSkillTable_tempArray = dataRow[33].ToString().Split(',');
		for( int i = 0; i < LevelUpSkillTable_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(LevelUpSkillTable_tempArray[i]);
			LevelUpSkillTable.Add(temp);
		}
		OpenStage = ulong.Parse(dataRow[34].ToString());
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

public class UpgradeDataTable_List : DataTableListBase
{
	public const string NAME = "Upgrade";
	public const string DATAFILENAME = "UpgradeData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			UpgradeDataTable data = new UpgradeDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class UpgradeDataTable : DataTableBase
{
	public string Name;
	public string Description;
	public int UpgradeValue;
	public int Probability;
	public string Floor;
	public void Load(DataRow dataRow)
	{
		Name = dataRow[1].ToString();
		Description = dataRow[2].ToString();
		UpgradeValue = int.Parse(dataRow[3].ToString());
		Probability = int.Parse(dataRow[4].ToString());
		Floor = dataRow[5].ToString();
	}
}

public class WarpDataTable_List : DataTableListBase
{
	public const string NAME = "Warp";
	public const string DATAFILENAME = "WarpData.dat";
	public override void Load(DataTable dataTable)
	{
		foreach(DataRow dr in dataTable.Rows)
		{
			ulong serialNo = ulong.Parse(dr[0].ToString());
			WarpDataTable data = new WarpDataTable();
			data.Load(dr);
			DataList.Add(serialNo, data);
		}
	}

}

public class WarpDataTable : DataTableBase
{
	public int Value;
	public int GemCount;
	public void Load(DataRow dataRow)
	{
		Value = int.Parse(dataRow[1].ToString());
		GemCount = int.Parse(dataRow[2].ToString());
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
		Count = new List<ushort>();
		string [] Count_tempArray = dataRow[2].ToString().Split(',');
		for( int i = 0; i < Count_tempArray.Length; ++i)
		{
			ushort temp = ushort.Parse(Count_tempArray[i]);
			Count.Add(temp);
		}
		WaveStartDelay = int.Parse(dataRow[3].ToString());
		NextWave = ulong.Parse(dataRow[4].ToString());
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
	public byte WorldMoveAnimIndex;
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
		WorldMoveAnimIndex = byte.Parse(dataRow[4].ToString());
		StageList = new List<ulong>();
		string [] StageList_tempArray = dataRow[5].ToString().Split(',');
		for( int i = 0; i < StageList_tempArray.Length; ++i)
		{
			ulong temp = ulong.Parse(StageList_tempArray[i]);
			StageList.Add(temp);
		}
		BossSerialNo = ulong.Parse(dataRow[6].ToString());
		BossLevel = int.Parse(dataRow[7].ToString());
		EnhancementStone = int.Parse(dataRow[8].ToString());
		Description = dataRow[9].ToString();
		BossFailTime = int.Parse(dataRow[10].ToString());
	}
}


