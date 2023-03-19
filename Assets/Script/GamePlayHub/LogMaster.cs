using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Main;

namespace GamePlayHub
{
    public class LogMaster
    {
        StringBuilder _LogBuilder;
        private static LogMaster _LogMaster;
        Text _LogTXT;
        public GameObject View;
        StringBuilder _LineBuilder;

        private LogMaster()
        {
            _LogBuilder = new StringBuilder("");
            _LogTXT = GameObject.FindGameObjectWithTag("Log").GetComponent<Text>();
            View = GameObject.Find("Scroll View");
            _LogTXT.text = _LogBuilder.ToString();
            _LineBuilder = new StringBuilder();
        }

        public static LogMaster GetInstance()
        {
            if (_LogMaster == null)
                _LogMaster = new LogMaster();
            return _LogMaster;
        }

        public void ClearLog()
        {
            _LineBuilder.Clear();
            _LogBuilder.Clear();
            _LogTXT.text = "";
        }

        public void AddComponent_CastSkill(ClientPlayer clientPlayer, int characterNameCode)
        {
            AddComponent(clientPlayer);
            AddComponent("發動技能");
            string str = DataMaster.GetInstance().GetCharacterInfo(characterNameCode).SkillName;
            string color = "4FF1D8";
            _LineBuilder.Append(GetUGUIString(str, color));
            AddComponent("。");
        }

        public void AddComponent(ClientPlayer clientPlayer)
        {
            string str = (clientPlayer.IsSelf) ? "你" : DataMaster.GetInstance().GetCharacterInfo(clientPlayer.CharacterNameCode).CharacterName;
            string color = "F3ABEB";
            _LineBuilder.Append(GetUGUIString(str, color));
        }

        public void AddComponent(string str)
        {
            _LineBuilder.Append(str);
        }

        public void AddComponent(CardInfo cardInfo)
        {
            string str;
            string color;
            if (cardInfo.IsPublic)
            {
                str = Func.GetChineseNameOfPowerType(cardInfo.PublicInfo.PowerType) + Func.GetChineseNameOfPublicCardClass(cardInfo.PublicInfo.PublicCardClass);
                color = Func.GetColorStringOfPowerType(cardInfo.PublicInfo.PowerType);
            } else
            {
                str = cardInfo.StrategyInfo.StrategyName;
                color = "F3ABEB";
            }
            _LineBuilder.Append(GetUGUIString(str, color));
        }

        public void Log()
        {
            _LogBuilder.Append(_LineBuilder.ToString());
            _LineBuilder.Clear();
            _LineBuilder.Append('\n');
            _LogTXT.text = _LogBuilder.ToString();
        }

        string GetUGUIString(string str, string color)
        {
            return "<color=#" + color + ">" + str + "</color>";
        }
    }
}
