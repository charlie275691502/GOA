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
using Rayark.Mast;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;


namespace Rayark.Hello
{
	public class Main : MonoBehaviour {


		private readonly Executor _executor = new Executor();
	
		void Start () {
		
			// Make this game object not destroyed after loading scenes
			Object.DontDestroyOnLoad(this.gameObject);
		
			// Resume once after scene loaded, there we can have main coroutine doing something
			// before any local logic within a scene
			SceneManager.sceneLoaded += (_, __) => _executor.Resume(0);

		
			// Let's start our application
			_executor.Add(_Main(_executor));
		}
	
		void Update () {
			_executor.Resume(Time.deltaTime);
		}


		static IEnumerator _Main(IExecutor executor)
		{
		
			// Create top level container that host all cross scene resources
			DiContainer container = new DiContainer();
			
			// Make root executor available to all scenes to allow 
			// some logic may be executed in parallel with _Main
			container.Bind<IExecutor>().FromInstance(executor);

			// User profile is the global basic information about the user
			// It can be access from all scene.
			container.Bind<UserProfile>().FromInstance(new UserProfile()
			{
				Name = "Alvin"
			});
			
			while (true)
			{
				// Let's start menu.
				// Create a sub container to avoid local resources leaked outside
				// of this scene.
				yield return _Menu(container.CreateSubContainer());
				
				// Let's play a game
				yield return _Gameplay(container.CreateSubContainer());

				// Let's see the result of the game
				yield return _Ranking(container.CreateSubContainer());

			}
		}


		// TODO: Rewrite in Monad form
		static IEnumerator _Menu(DiContainer container)
		{
			// Load scene asynchronously.
			// In real case, we usually load a scene including loading screen
			// before with asynchronously
			var op = SceneManager.LoadSceneAsync("menu");

			op.allowSceneActivation = false;

			while (op.progress < 0.9f)
				yield return null;

			op.allowSceneActivation = true;

			// loaded but not awake

			while (!op.isDone)
				yield return null;
			
			var facade = Object.FindObjectOfType<MenuFacade>();
			yield return facade.Run(container);
		}


		static IEnumerator _Gameplay(DiContainer container)
		{
			var op = SceneManager.LoadSceneAsync("gameplay");

			op.allowSceneActivation = false;

			while (op.progress < 0.9f)
				yield return null;

			op.allowSceneActivation = true;

			// loaded but not awake

			while (!op.isDone)
				yield return null;

			var facade = Object.FindObjectOfType<GameplayFacade>();
			yield return facade.Run(container);
		}

		static IEnumerator _Ranking(DiContainer container)
		{
			var op = SceneManager.LoadSceneAsync("ranking");

			op.allowSceneActivation = false;

			while (op.progress < 0.9f)
				yield return null;

			op.allowSceneActivation = true;

			// loaded but not awake

			while (!op.isDone)
				yield return null;

			var facade = Object.FindObjectOfType<RankingFacade>();
			yield return facade.Run(container);		}
	}
}
