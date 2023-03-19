using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Main;

namespace GamePlayHub
{
    public class CharacterHandler : MonoBehaviour
    {
        private int _NameCode = -1;
        public int NameCode { get { return _NameCode; } set {
                if (_NameCode == value) _NameCode = -1;
                else _NameCode = value;

                PackGMO.SetActive(_NameCode != -1);
                if (_NameCode == -1) return;
                CharacterInfo characterInfo = DataMaster.GetInstance().GetCharacterInfo(_NameCode);

                CharacterNameTXT.text = characterInfo.CharacterName;
                SkillNameTXT.text = characterInfo.SkillName;
                SkillDescriptionTXT.text = characterInfo.SkillDescription;
            }
        }
        public GamePlayManager GamePlayManager;
        public GameObject PackGMO;
        public Text CharacterNameTXT;
        public Text SkillNameTXT;
        public Text SkillDescriptionTXT;

        public void Init(int serial, GamePlayManager gamePlayManager, int characterNameCode)
        {
            GamePlayManager = gamePlayManager;
            transform.localPosition = new Vector3(serial * 130 - 130, (serial == 1) ? 311 : 11, 0);
            NameCode = characterNameCode;
        }

        private void OnMouseUp()
        {
            GamePlayManager.DecideCharacter(_NameCode);
        }
    }
}
