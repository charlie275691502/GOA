/* (C)2019 Rayark Inc. - All Rights Reserved
 * Rayark Confidential
 *
 * NOTICE: The intellectual and technical concepts contained herein are
 * proprietary to or under control of Rayark Inc. and its affiliates.
 * The information herein may be covered by patents, patents in process,
 * and are protected by trade secret or copyright law.
 * You may not disseminate this information or reproduce this material
 * unless otherwise prior agreed by Rayark Inc. in writing.
 */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rayark.Hello
{
    public class MenuView : MonoBehaviour, IMenuView
    {
        [SerializeField] private Text _textName;

        [SerializeField] private Button _startButton;
        public void Start()
        {
            _startButton.onClick.AddListener(
                () => OnStartClicked?.Invoke() );
        }
        
        public void ShowName(string name)
        {
            _textName.text = name;
        }

        public event Action OnStartClicked;
    }
}