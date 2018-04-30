using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public enum DiceGameResult
{
PlayerVictory,
RivalVictory,
	Draw
}
namespace DiceGame
{
	public class GameManager : MonoBehaviour
	{

		public Color attackColor;
		public Color shieldColor;
		public Color selfDamageColor;

		public static DiceGame.GameManager Instance { get; private set; }
		private const int DEF_DICE_EACH_TURN = 5;
		private const int MAX_HP = 10;
		private const int DICE_ARRAY_SIZE = 15;
		private int diceForTurn;
		private int playerIndex;
		private int rivalIndex;
		[SerializeField] private Transform playerThrow;
		[SerializeField] private Transform rivalThrow;
		[SerializeField] private Text HP;
//		[SerializeField] private Text Shield;
		[SerializeField] private Image[] shields;
		[SerializeField] private Text rivalHP;
//		[SerializeField] private Text rivalShield;
		[SerializeField] private ParticleSystem playerHurtEffect;
		[SerializeField] private ParticleSystem rivalHurtEffect;
		[SerializeField] private ParticleSystem[] shieldBuildEffects;
		[SerializeField] private ParticleSystem[] rivalShieldBuildEffects;
		[SerializeField] private ParticleSystem[] shieldHitEffects;
		[SerializeField] private ParticleSystem[] otherShieldHitEffects;
		[SerializeField] private Image[] rivalShields;
		[SerializeField] private Image HPbar;
		[SerializeField] private Image rivalHPbar;
		[SerializeField] private Text playerBank;
		[SerializeField] private Text rivalBank;
		[SerializeField] private Button rollCubes;
		[SerializeField] private Button passTurn;
		[SerializeField] private Canvas HUD;
		public Animator canvasAnimator;
//		[SerializeField] private Canvas choiceMenu;
		private GameObject[] currentCubes;
		private int skullCounter;
		private int swordsCounter;
		private int shieldCounter;
		private int reRollCounter;
		private int returnCount;
		bool isPlayerTurn;
		bool isPlayerDone;
		bool isRivalDone;
		[HideInInspector]
		public int HPVAL, rivalHPVAL;
		private int ShieldVAL, rivalShieldVAL;
		public DiceBoard diceBoard;

		public bool playOnStart = true;

		public UnityEvent onGameStart;
		public DiceGameVictoryEvent onGameEnd;

		public Sprite lockedShield;
		public Sprite unlockedShield;

		void Awake()
		{
			Instance = this;
		}

		private void Start ()
		{

			DOTween.Init (false, true, LogBehaviour.ErrorsOnly);
			HUD.gameObject.SetActive (false);

//			ChooseSum ();
		}

		public void ChooseSum()
		{
			diceForTurn = DEF_DICE_EACH_TURN;
			playerIndex = 0;
			rivalIndex = 0;
			skullCounter = 0;
			swordsCounter = 0;
			shieldCounter = 0;
			reRollCounter = 0;
			returnCount = 0;
			isPlayerDone = false;
			isRivalDone = false;
			HPVAL = MAX_HP; 
			rivalHPVAL = MAX_HP;
			ShieldVAL = 0; 
			rivalShieldVAL = 0;
//			choiceMenu.gameObject.SetActive (false);
			HUD.gameObject.SetActive (true);
			isPlayerTurn = (Random.value > 0.5f);
            HP.text = HPVAL.ToString() + " / " + MAX_HP;
            rivalHP.text = rivalHPVAL.ToString() + " / " + MAX_HP;
            HPbar.fillAmount = calcHP (true);
			rivalHPbar.fillAmount = calcHP (false);
			playerBank.text = DICE_ARRAY_SIZE.ToString ();
			rivalBank.text = DICE_ARRAY_SIZE.ToString ();
			HighLightCurrentPlayerCubes ();
			onGameStart.Invoke ();

			canvasAnimator.SetTrigger ("Enter");

			RefreshShield (true);
			RefreshShield (false);
			if (playOnStart) {
				if (!isPlayerTurn) {
					ButtonStatus (false);
					//				AITurn ();
					StartCoroutine (AITurn (1.4f));
				} else {
					ButtonStatus (true);
				}
			} else {
				ButtonStatus (false);
//                StartCoroutine(AITurn(1.4f));
            }
		}

		public void PlayAgain()
		{
			for (int i = playerIndex; i < DiceBoard.PlayerCubes.Length; i++) {
				Destroy (DiceBoard.PlayerCubes [i]);
			}
			for (int i = rivalIndex; i < DiceBoard.RivalCubes.Length; i++) {
				Destroy (DiceBoard.RivalCubes [i]);
			}

			diceBoard.CreateDice ();
//			canvasAnimator.SetTrigger ("Exit");
			GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Played Again",0);
//			HUD.gameObject.SetActive (false);

//			choiceMenu.gameObject.SetActive (true);
		}

		private float calcHP (bool isPlayer)
		{
			if (isPlayer) {
				return (float)HPVAL / MAX_HP;
			} else {
				return (float)rivalHPVAL / MAX_HP;
			}
		}

		private void ButtonStatus (bool turnStatus)
		{
			rollCubes.interactable = turnStatus;
			passTurn.interactable = turnStatus;
		}

		public void RollDice ()
		{
			BoardFloor.ResetCubes ();
			ButtonStatus (false);
			if (playerIndex + DEF_DICE_EACH_TURN > DICE_ARRAY_SIZE) {
				diceForTurn = DICE_ARRAY_SIZE - playerIndex;
			} else {
				diceForTurn = DEF_DICE_EACH_TURN;
			}
			currentCubes = new GameObject[diceForTurn];
			int i;
			int ccIndex = 0;
			for (i = playerIndex; i < playerIndex + diceForTurn; i++) {
				currentCubes [ccIndex] = DiceBoard.PlayerCubes [i];
				currentCubes [ccIndex].GetComponent<Dice> ().highlighter.ConstantOffImmediate ();
				ForceThrow (DiceBoard.PlayerCubes [i]);
				ccIndex++;
			}
			playerIndex = i;
			playerBank.text = (DICE_ARRAY_SIZE - playerIndex).ToString ();
			StartCoroutine (CheckCubes ());

			StartCoroutine (PlayDiceRollDelay ());
		}

		IEnumerator PlayDiceRollDelay()
		{
			yield return new WaitForSeconds (0.15f);
			MiniGamesSceneSoundManager.Instance.PlayDiceRoll ();
		}

		private IEnumerator CheckCubes ()
		{
			yield return new WaitForSeconds (1.5f);
			if (BoardFloor.cubes.Count < currentCubes.Length) {
				for (int i = 0; i < currentCubes.Length; i++) {
					if (currentCubes [i].GetComponent<Dice> ().ReRolled == 3) {
						ForceThrow (currentCubes [i]);
						currentCubes [i].GetComponent<Dice> ().ReRolled = 0;
					} else if (!currentCubes [i].GetComponent<Dice> ().HasLanded) {
						currentCubes [i].GetComponent<Rigidbody> ().AddForce (Vector3.up * 5, ForceMode.Impulse);
						currentCubes [i].GetComponent<Rigidbody> ().AddTorque (new Vector3 (Random.Range (0, 500), Random.Range (0, 500), Random.Range (0, 500)));
						currentCubes [i].GetComponent<Dice> ().ReRolled++;
					}
				}
				StartCoroutine (CheckCubes ());
			} else {
				CalcDice ();
			}
		}

		private void CalcDice ()
		{
			skullCounter = 0;
			swordsCounter = 0;
			shieldCounter = 0;
			reRollCounter = 0;

			for (int i = 0; i < currentCubes.Length; i++) {
				switch (currentCubes [i].GetComponent<Dice> ().ValueAfterThrow) {
				case (int)cubeValues.Swords:
					swordsCounter++;
					break;
				case (int)cubeValues.Skull:
					skullCounter++;
					break;
				case (int)cubeValues.Reroll:
					reRollCounter++;
					break;
				case (int)cubeValues.Shield:
					shieldCounter++;
					break;
				case (int)cubeValues.None:
					print ("error with cube value");
					break;
				}
			}

			if (reRollCounter > 0) {
				RaiseDice ((int)cubeValues.Reroll);
			} else if (shieldCounter > 0) {
				RaiseDice ((int)cubeValues.Shield);
			} else if (skullCounter > 0) {
				RaiseDice ((int)cubeValues.Skull);
			} else if (swordsCounter > 0) {
				RaiseDice ((int)cubeValues.Swords);
			} else {
				EndTurn ();
			}
		}

		private void ReturnDice (GameObject dice)
		{
			Sequence seq = DOTween.Sequence ().SetAutoKill (true);
			MiniGamesSceneSoundManager.Instance.PlayReRoll ();
			dice.GetComponent<Dice> ().HasLanded = false;
			if (isPlayerTurn) {
				playerIndex--;
				dice.GetComponent<Rigidbody> ().isKinematic = true;
				Vector3 midVector = new Vector3 (DiceBoard.PlayerVectors [playerIndex].x, DiceBoard.PlayerVectors [playerIndex].y + 0.35f, DiceBoard.PlayerVectors [playerIndex].z);
				Vector3 endVector = new Vector3 (DiceBoard.PlayerVectors [playerIndex].x, DiceBoard.PlayerVectors [playerIndex].y + 0.00f, DiceBoard.PlayerVectors [playerIndex].z);
				seq.Append (dice.transform.DOMove (midVector, 1f));
				seq.Join (dice.transform.DORotate (diceBoard.cubeObject.transform.rotation.eulerAngles, 1f));
				seq.Append (dice.transform.DOMove (endVector, 1f).OnComplete (() => {
					playerBank.text = (DICE_ARRAY_SIZE - playerIndex).ToString();
					dice.GetComponent<Rigidbody> ().isKinematic = false;
					returnCount++;
					if (returnCount == reRollCounter) {
						returnCount = 0;
						DiceLogic ((int)cubeValues.Reroll);
					}
				}));
				DiceBoard.PlayerCubes [playerIndex] = dice;
			} else {
				rivalIndex--;
				dice.GetComponent<Rigidbody> ().isKinematic = true;
				Vector3 midVector = new Vector3 (DiceBoard.RivalVectors [rivalIndex].x, DiceBoard.RivalVectors [rivalIndex].y + 0.15f, DiceBoard.RivalVectors [rivalIndex].z);
				Vector3 endVector = new Vector3 (DiceBoard.RivalVectors [rivalIndex].x, DiceBoard.RivalVectors [rivalIndex].y + 0.00f, DiceBoard.RivalVectors [rivalIndex].z);
				seq.Append (dice.transform.DOMove (midVector, 1f));
				seq.Join (dice.transform.DORotate (diceBoard.cubeObject.transform.rotation.eulerAngles, 1f));
				seq.Append (dice.transform.DOMove (endVector, 1f).OnComplete (() => {
					rivalBank.text = (DICE_ARRAY_SIZE - rivalIndex).ToString();
					dice.GetComponent<Rigidbody> ().isKinematic = false;
					returnCount++;
					if (returnCount == reRollCounter) {
						returnCount = 0;
						DiceLogic ((int)cubeValues.Reroll);
					}
				}));
				DiceBoard.RivalCubes [rivalIndex] = dice;
			}

		}

		private void DealDamage (int doWhat)
		{
			int health;
			int shield;
			int otherHealth;
			int otherShield;
			ParticleSystem hurtEffect;
			ParticleSystem otherHurtEffect;

			if (isPlayerTurn) {
				health = HPVAL;
				shield = ShieldVAL;
				otherHealth = rivalHPVAL;
				otherShield = rivalShieldVAL;
				hurtEffect = playerHurtEffect;
				otherHurtEffect = rivalHurtEffect;
			} else {
				health = rivalHPVAL;
				shield = rivalShieldVAL;
				otherHealth = HPVAL;
				otherShield = ShieldVAL;
				hurtEffect = rivalHurtEffect;
				otherHurtEffect = playerHurtEffect;
			}

			if (doWhat == (int)cubeValues.Skull) {
				if (shield > 0) {
					MiniGamesSceneSoundManager.Instance.PlayArmorBreak ();
				}
				if (skullCounter > shield) {
					hurtEffect.Play ();
					MiniGamesSceneSoundManager.Instance.PlaySelfHit ();
					health -= skullCounter - shield;
                    if (health < 0) { health = 0; }
					shield = 0;
				} else {
					shield -= skullCounter;
				}

			}

			if (doWhat == (int)cubeValues.Swords) {
				if (otherShield > 0) {
					MiniGamesSceneSoundManager.Instance.PlayArmorBreak ();
				}
				if (swordsCounter > otherShield) {
					otherHurtEffect.Play ();
					MiniGamesSceneSoundManager.Instance.PlayLifeHurt ();
					otherHealth -= swordsCounter - otherShield;
                    if (otherHealth < 0) { otherHealth = 0; }
                    otherShield = 0;
				} else {
					otherShield -= swordsCounter;
				}
			}

			if (isPlayerTurn) {
				HPVAL = Mathf.Clamp (health, 0, int.MaxValue);
				HP.text = HPVAL.ToString()  + " / " + MAX_HP;
				ShieldVAL = shield;
//				Shield.text = ShieldVAL.ToString ();
				RefreshShield(true);
				rivalHPVAL = otherHealth;
				rivalHP.text = rivalHPVAL.ToString () + " / " + MAX_HP;
				rivalShieldVAL = otherShield;
				RefreshShield (false);
//				rivalShield.text = rivalShieldVAL.ToString ();
			} else {
				HPVAL = Mathf.Clamp (otherHealth, 0, int.MaxValue);
				HP.text = HPVAL.ToString () + " / " + MAX_HP;
				ShieldVAL = otherShield;
				RefreshShield (true);
//				Shield.text = ShieldVAL.ToString ();
				rivalHPVAL = health;
				rivalHP.text = rivalHPVAL.ToString () + " / " + MAX_HP;
				rivalShieldVAL = shield;
				RefreshShield (false);
//				rivalShield.text = rivalShieldVAL.ToString ();
			}
			HPbar.fillAmount = calcHP (true);
			rivalHPbar.fillAmount = calcHP (false);
		}

		void RefreshShield(bool player)
		{
			Image[] shieldsToRefresh = player ? shields : rivalShields;
			ParticleSystem[] buildEffects = player ? shieldBuildEffects : rivalShieldBuildEffects;
			ParticleSystem[] hitEffects = player ? shieldHitEffects : otherShieldHitEffects;
			int shieldValue = player ? ShieldVAL : rivalShieldVAL;
			for (int i = 0; i < shieldsToRefresh.Length; i++) {
				if (i < shieldValue) {
					if (shieldsToRefresh [i].sprite != unlockedShield) {
						shieldsToRefresh [i].sprite = unlockedShield;
						buildEffects [i].Play ();
					}
				} else {
					if (shieldsToRefresh [i].sprite != lockedShield) {
						
						shieldsToRefresh [i].sprite = lockedShield;
						hitEffects [i].Play ();
					}
				}
			}
		}

//		void RefreshBank(bool player)
//		{
//			Text bankToRefresh = player ? playr
//		}


		IEnumerator AITurn(float delay)
		{
			yield return new WaitForSeconds (delay);
			AITurn ();
		}

		private void AITurn ()
		{
			isPlayerTurn = false;
			HighLightCurrentPlayerCubes ();
			if (rivalIndex + DEF_DICE_EACH_TURN > DICE_ARRAY_SIZE) {
				diceForTurn = DICE_ARRAY_SIZE - rivalIndex;
			} else {
				diceForTurn = DEF_DICE_EACH_TURN;
			}
			if (diceForTurn > 0) {
				currentCubes = new GameObject[diceForTurn];
				int i;
				int ccIndex = 0;
				for (i = rivalIndex; i < rivalIndex + diceForTurn; i++) {
					currentCubes [ccIndex] = DiceBoard.RivalCubes [i];
					currentCubes [ccIndex].GetComponent<Dice> ().highlighter.ConstantOffImmediate ();
					ForceThrow (DiceBoard.RivalCubes [i]);
					ccIndex++;
				}
				rivalIndex = i;
				rivalBank.text = (DICE_ARRAY_SIZE - rivalIndex).ToString ();
				StartCoroutine (CheckCubes ());
			} else
				EndTurn ();
		}

		private void DeleteCurrDice ()
		{
			for (int i = 0; i < currentCubes.Length; i++) {
				if (currentCubes [i].GetComponent<Dice> ().ValueAfterThrow != (int)cubeValues.Reroll) {
					Destroy (currentCubes [i]);
				}
			}
		}

		private void ForceThrow (GameObject cube)
		{
			Vector3 throwDir;
			if (isPlayerTurn) {
				cube.transform.position = playerThrow.position;
				throwDir = playerThrow.transform.forward;
			} else {
				cube.transform.position = rivalThrow.position;
				throwDir = rivalThrow.transform.forward;
			}
			cube.transform.position += new Vector3 (Random.Range (-0.1f, 0.1f), Random.Range (-0.1f, 0.2f), 0);
			cube.transform.rotation = Random.rotation;
			cube.GetComponent<Rigidbody> ().AddTorque (new Vector3 (Random.Range (250, 1000), Random.Range (250, 1000), Random.Range (250, 1000)));
			cube.GetComponent<Rigidbody> ().AddForce (throwDir, ForceMode.VelocityChange);
        
		}

		private void CheckWin ()
		{
			if (HPVAL == 0 && rivalHPVAL == 0) {
				onGameEnd.Invoke (DiceGameResult.Draw);
				GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Game End - Draw",0);
				print ("Draw");
			} else if (HPVAL == 0) {
				onGameEnd.Invoke (DiceGameResult.RivalVictory);
				GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Game End - Defeat",0);
				print ("Enemy wins");
			} else if (rivalHPVAL == 0) {
				onGameEnd.Invoke (DiceGameResult.PlayerVictory);
				GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Game End - Victory",0);
				AchievementsManager.Instance.AddStat (AchievementsManager.NumOfBattleDiceVictoriesID, 1);
				print ("Player wins");
			} else if (HPVAL < rivalHPVAL) {
				GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Game End - Defeat",0);
				onGameEnd.Invoke (DiceGameResult.RivalVictory);
				print ("Enemy wins");
			} else if (HPVAL > rivalHPVAL) {
				onGameEnd.Invoke (DiceGameResult.PlayerVictory);
				GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Game End - Victory",0);
				print ("Player wins");
			} else {
				onGameEnd.Invoke (DiceGameResult.Draw);
				GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Game End - Draw",0);
				print ("DRAW");
			}
//			choiceMenu.gameObject.SetActive (true);
		}

		public void PassTurn ()
		{
			GoogleAnalyticsV4.instance.LogEvent ("Mini Games", "Battle Dice", "Stopped",0);
			isPlayerDone = true;
			ButtonStatus (false);
			if (isPlayerDone && isRivalDone) {
				CheckWin ();
			} else {
				AITurn ();
			}
		}

		private void RaiseDice (int arisen)
		{
			Color glowColor = new Color ();
			switch (arisen) {
			case (int)cubeValues.Swords:
				glowColor = attackColor;
//				glowColor = Color.red;
				break;
			case (int)cubeValues.Skull:
//				glowColor = Color.gray;
				glowColor = selfDamageColor;
				break;
			case (int)cubeValues.Shield:
//				glowColor = Color.yellow;
				glowColor = shieldColor;
				break;
			case (int)cubeValues.None:
				print ("error with cube value");
				break;
			}
			Sequence seq = DOTween.Sequence ();
			if (arisen != (int)cubeValues.Reroll) {
				seq = DOTween.Sequence ().SetAutoKill (false).OnComplete (() => DiceLogic (arisen));
			}
			for (int i = 0; i < currentCubes.Length; i++) {
				if ((int)currentCubes [i].GetComponent<Dice> ().ValueAfterThrow == arisen) {
					if (arisen == (int)cubeValues.Reroll) {
						ReturnDice (currentCubes [i]);
					} else {
						currentCubes [i].GetComponent<Dice> ().Halo.color = glowColor;
						seq.Join (currentCubes [i].transform.DOMoveY (currentCubes [i].transform.position.y + 0.05f, 1f)).SetDelay (0.1f);
//						currentCubes [i].GetComponent<Dice> ().highlighter.ConstantOn (glowColor,1f);
						currentCubes[i].GetComponent<Dice>().HighlightAndDehighlight(glowColor);
//						seq.Join (currentCubes [i].GetComponent<Dice> ().Halo.DOIntensity (0.6f, 2f).SetDelay (0.5f));
						currentCubes [i].GetComponent<Rigidbody> ().isKinematic = true;
						;
					}
				}
			} 
		}

		private void DiceLogic (int cubeValue)
		{
			switch (cubeValue) {
			case (int)cubeValues.Swords:
				DealDamage (cubeValue);
				StartCoroutine (LowerAndContinue (cubeValue));
				break;
			case (int)cubeValues.Skull:
				DealDamage (cubeValue);
				StartCoroutine (LowerAndContinue (cubeValue));
				break;
			case (int)cubeValues.Shield:
				if (isPlayerTurn) {
					MiniGamesSceneSoundManager.Instance.PlayArmorBuild ();
					ShieldVAL += shieldCounter;
					if (ShieldVAL > 5) {
						ShieldVAL = 6;
					}
					RefreshShield (true);
//					Shield.text = (ShieldVAL).ToString ();
				} else {
					MiniGamesSceneSoundManager.Instance.PlayArmorBuild ();
					rivalShieldVAL += shieldCounter;
					if (rivalShieldVAL > 5) {
						rivalShieldVAL = 6;
					}
					RefreshShield (false);
//					rivalShield.text = (rivalShieldVAL).ToString ();
				}
				StartCoroutine (LowerAndContinue (cubeValue));
				break;
			case (int)cubeValues.Reroll:
				if (shieldCounter > 0) {
					RaiseDice ((int)cubeValues.Shield);
				} else if (skullCounter > 0) {
					RaiseDice ((int)cubeValues.Skull);
				} else if (swordsCounter > 0) {
					RaiseDice ((int)cubeValues.Swords);
				} else {
					EndTurn ();
				}
				break;
			case (int)cubeValues.None:
				print ("error with cube value");
				break;
			}    
		}

		private IEnumerator LowerAndContinue (int cubeValue)
		{
			yield return new WaitForSeconds (0.25f);
			DOTween.PlayBackwardsAll ();

			yield return new WaitForSeconds (1.5f);
			switch (cubeValue) {
			case (int)cubeValues.Swords:
				EndTurn ();
				break;
			case (int)cubeValues.Skull:
				if (swordsCounter > 0) {
					RaiseDice ((int)cubeValues.Swords);
				} else {
					EndTurn ();
				}
				break;
			case (int)cubeValues.Shield:
				if (skullCounter > 0) {
					RaiseDice ((int)cubeValues.Skull);
				} else if (swordsCounter > 0) {
					RaiseDice ((int)cubeValues.Swords);
				} else {
					EndTurn ();
				}
				break;
			case (int)cubeValues.None:
				print ("error with cube value");
				break;
			}
		}

		private void EndTurn ()
		{
			DeHighLightAllCubes ();
			BoardFloor.ResetCubes ();
			DeleteCurrDice ();
			if (isPlayerTurn) {
				if (playerIndex == DICE_ARRAY_SIZE) {
					isPlayerDone = true;
				}
				if ((isPlayerDone && isRivalDone) || (HPVAL <= 0) || (rivalHPVAL <= 0)) {
					CheckWin ();
				} else {
					if (!isRivalDone) {
						AITurn ();
					} else {
						isPlayerTurn = true;
						ButtonStatus (true);
						HighLightCurrentPlayerCubes ();
					}
				
				}
			} else {
				if (rivalIndex == DICE_ARRAY_SIZE) {
					isRivalDone = true;
				}
				if ((isPlayerDone && isRivalDone) || (HPVAL <= 0) || (rivalHPVAL <= 0)) {
					CheckWin ();
				} else {
					if (isPlayerDone) {
						AITurn ();
					} else {
						isPlayerTurn = true;
						ButtonStatus (true);
						HighLightCurrentPlayerCubes ();
					}
				}
			}
		}

		public void HighLightCurrentPlayerCubes()
		{
			GameObject[] cubesToHighlight;
			if (isPlayerTurn)
				cubesToHighlight = DiceBoard.PlayerCubes;
			else
				cubesToHighlight = DiceBoard.RivalCubes;
			
			foreach (GameObject cube in cubesToHighlight) {
				if (cube != null) {
					cube.GetComponent<Dice> ().highlighter.seeThrough = false;
					cube.GetComponent<Dice> ().highlighter.ConstantOn (Color.green);
				}
			
			}


				
		}

		public void DeHighLightAllCubes()
		{
			foreach (GameObject cube in DiceBoard.PlayerCubes) {
				if(cube != null)
				cube.GetComponent<Dice> ().highlighter.ConstantOffImmediate ();
			}

			foreach (GameObject cube in DiceBoard.RivalCubes) {
				if(cube != null)
				cube.GetComponent<Dice> ().highlighter.ConstantOffImmediate ();
			}
		}

		public void ContinuePlay()
		{
			if (!isPlayerTurn) {
				ButtonStatus (false);
				//				AITurn ();
				StartCoroutine (AITurn (1.4f));
			} else {
				ButtonStatus (true);
			}
		} 
	}
}
