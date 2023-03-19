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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Coroutine = Rayark.Mast.Coroutine;

namespace Rayark.Hello
{
	public class GameplayFacade : MonoBehaviour {

		public IEnumerator Run(DiContainer container)
		{
			yield return Coroutine.Sleep(5.0f);
			yield break;
		}
	}
}