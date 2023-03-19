using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlayHub
{
    public class CardChooserHandler : MonoBehaviour
    {
        public GameObject Pack;
        public CardHandler Card;
        PowerType _PowerType;
        public PowerType PowerType
        {
            get { return _PowerType; }
            set {
                _PowerType = value;
                UpdateCard();
            }
        } 
        int _Power;
        public int Power
        {
            get { return _Power; }
            set
            {
                _Power = value;
                UpdateCard();
            }
        }

        private bool _IsShow;
        public bool IsShow{
            get { return _IsShow; }
            set {
                _IsShow = value;
                if (value)
                {
                    Power = 1;
                    PowerType = PowerType.Wealth;
                }
                Pack.SetActive(value);
            }
        }

        public void AddPowerType(int x)
        {
            PowerType = (PowerType)(((int)PowerType + x + 4) % 4);
        }

        public void AddPower(int x)
        {
            Power = (Power + x + 9 - 1) % 9 + 1;
        }

        void UpdateCard()
        {
            Card.NameCode = DataMaster.GetInstance().GetNameCodesOfPublicCard(PowerType, Power)[0];
        }
    }
}
