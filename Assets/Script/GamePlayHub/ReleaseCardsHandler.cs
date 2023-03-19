using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rayark.Mast;

namespace GamePlayHub
{
    public class ReleaseCardsHandler : MonoBehaviour
    {
        List<GameObject> _ReleaseCards;
        List<CardAnimationType> _CardAnimationType;
        public GameObject CardPRF;
        public GamePlayManager GamePlayManager;

        public Vector3 OffsetFromProfile0;

        Vector3 OffsetFromDeckHandler = new Vector3(643.5f, -236.2f, 0);
        Vector3 GraveLocalPositionOfProfile0 = new Vector3(982, -357, 0);
        Vector3 DrawLocalPositionOfProfile0  = new Vector3(982, -142, 0);

        Executor _Executor;
        float travelTime = 0.3f;

        IEnumerator MoveEnemyToGrave(GameObject card)
        {
            Vector3 startPosition = card.transform.localPosition;
            Vector3 goalPosition = GraveLocalPositionOfProfile0 + OffsetFromProfile0;
            Vector3 positionVelocity = (goalPosition - startPosition) / travelTime;

            Vector3 startScale = card.transform.localScale;
            Vector3 goalScale = Vector3.one * 1.25f;
            Vector3 scaleVelocity = (goalScale - startScale) / travelTime;

            for (float time = 0; time < travelTime; time += Time.deltaTime)
            {
                card.transform.localPosition = startPosition + positionVelocity * (time);
                card.transform.localScale = startScale + scaleVelocity * (time);
                yield return null;
            }

            GamePlayManager.GravePileHandler.Amount++;
            Destroy(card);
        }

        IEnumerator MoveDrawToEnemy(GameObject card)
        {
            GamePlayManager.DrawPileHandler.Amount--;

            Vector3 startPosition = card.transform.localPosition;
            Vector3 goalPosition = Vector3.zero;
            Vector3 positionVelocity = (goalPosition - startPosition) / travelTime;

            Vector3 startScale = card.transform.localScale;
            Vector3 goalScale = Vector3.one * 0.625f;
            Vector3 scaleVelocity = (goalScale - startScale) / travelTime;

            for (float time = 0; time < travelTime; time += Time.deltaTime)
            {
                card.transform.localPosition = startPosition + positionVelocity * (time);
                card.transform.localScale = startScale + scaleVelocity * (time);
                yield return null;
            }

            Destroy(card);
        }

        IEnumerator MoveDeckToEnemy(GameObject card)
        {
            Vector3 startPosition = card.transform.localPosition;
            Vector3 goalPosition = Vector3.zero;
            Vector3 positionVelocity = (goalPosition - startPosition) / travelTime;

            Vector3 startScale = card.transform.localScale;
            Vector3 goalScale = Vector3.one * 0.625f;
            Vector3 scaleVelocity = (goalScale - startScale) / travelTime;

            for (float time = 0; time < travelTime; time += Time.deltaTime)
            {
                card.transform.localPosition = startPosition + positionVelocity * (time);
                card.transform.localScale = startScale + scaleVelocity * (time);
                yield return null;
            }

            Destroy(card);
        }

        void Start()
        {
            _Executor = new Executor();
            _ReleaseCards = new List<GameObject>();
            _CardAnimationType = new List<CardAnimationType>();
        }

        void Update()
        {
            if (!_Executor.Finished)
                _Executor.Resume(Time.deltaTime);
        }

        public void AddReleaseCards(CardAnimationType cardAnimationType, params int[] releaseCards)
        {
            for (int i = 0; i < releaseCards.Length; i++)
            {
                GameObject gmo = null;
                if(cardAnimationType == CardAnimationType.EnemyToGrave)
                    gmo = InitEnemyToGraveGameObject(releaseCards[i]);
                else if (cardAnimationType == CardAnimationType.DrawToEnemy)
                    gmo = InitDrawToEnemyGameObject(releaseCards[i]);
                else if (cardAnimationType == CardAnimationType.OpenDeckToEnemy)
                    gmo = InitOpenDeckToEnemyGameObject(releaseCards[i]);
                else if (cardAnimationType == CardAnimationType.CoveredDeckToEnemy)
                    gmo = InitCoveredDeckToEnemyGameObject(releaseCards[i]);

                _ReleaseCards.Add(gmo);
                _CardAnimationType.Add(cardAnimationType);
            }
        }

        public IEnumerator PlayAnimation()
        {
            for(int i=0;i< _ReleaseCards.Count;i++)
            {
                if(_CardAnimationType[i] == CardAnimationType.EnemyToGrave)
                    _Executor.Add(MoveEnemyToGrave(_ReleaseCards[i]));
                if (_CardAnimationType[i] == CardAnimationType.DrawToEnemy)
                    _Executor.Add(MoveDrawToEnemy(_ReleaseCards[i]));
                if (_CardAnimationType[i] == CardAnimationType.OpenDeckToEnemy)
                    _Executor.Add(MoveDeckToEnemy(_ReleaseCards[i]));
                if (_CardAnimationType[i] == CardAnimationType.CoveredDeckToEnemy)
                    _Executor.Add(MoveDeckToEnemy(_ReleaseCards[i]));

            }
            _ReleaseCards.Clear();
            _CardAnimationType.Clear();

            while (!_Executor.Finished) yield return null;
        }

        GameObject InitEnemyToGraveGameObject(int nameCode)
        {
            GameObject gmo = Instantiate(CardPRF, transform);
            int x = _ReleaseCards.Count % 4;
            int y = _ReleaseCards.Count / 4;
            gmo.GetComponent<CardHandler>().Init(new Vector3(252 + x * 46, 40 - y * 68, 0), CardPlace.Play, -1, nameCode, GamePlayManager);
            gmo.transform.localScale = Vector3.one * 0.625f;
            return gmo;
        }

        GameObject InitDrawToEnemyGameObject(int nameCode)
        {
            GameObject gmo = Instantiate(CardPRF, transform);
            gmo.GetComponent<CardHandler>().Init(DrawLocalPositionOfProfile0 + OffsetFromProfile0, CardPlace.Play, -1, nameCode, GamePlayManager);
            gmo.transform.localScale = Vector3.one * 1.25f;
            return gmo;
        }

        GameObject InitOpenDeckToEnemyGameObject(int nameCode)
        {
            Vector3 localPosition = GetLocalPositionOfDeckHandlerByNameCode(nameCode);
            GameObject gmo = Instantiate(CardPRF, transform);
            gmo.GetComponent<CardHandler>().Init(localPosition + OffsetFromDeckHandler + OffsetFromProfile0, CardPlace.Play, -1, nameCode, GamePlayManager);
            gmo.transform.localScale = Vector3.one * 1.25f;
            gmo.GetComponent<CardHandler>().IsChosen = true;
            return gmo;
        }

        GameObject InitCoveredDeckToEnemyGameObject(int deckCardIndex)
        {
            Vector3 localPosition = GamePlayManager.DeckCardHandlers[deckCardIndex].gameObject.transform.localPosition;
            GameObject gmo = Instantiate(CardPRF, transform);
            gmo.GetComponent<CardHandler>().Init(localPosition + OffsetFromDeckHandler + OffsetFromProfile0, CardPlace.Play, -1, -1, GamePlayManager);
            gmo.transform.localScale = Vector3.one * 1.25f;
            gmo.GetComponent<CardHandler>().IsChosen = true;
            return gmo;
        }

        Vector3 GetLocalPositionOfDeckHandlerByNameCode(int nameCode)
        {
            for (int i = 0; i < GamePlayManager.DeckCardHandlers.Length; i++)
                if (GamePlayManager.DeckCardHandlers[i].NameCode == nameCode)
                    return GamePlayManager.DeckCardHandlers[i].gameObject.transform.localPosition;
            return Vector3.zero;
        }
    }
}
