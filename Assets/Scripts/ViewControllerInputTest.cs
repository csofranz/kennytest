using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewControllerInputTest : MonoBehaviour {

	public GameObject theEffect;
	public GameObject theSource; // where theEffect will be spawned
	public GameObject theTarget; // where it will hit

	public GameObject secondEffect;
	public GameObject thirdEffect;

	public GameObject glyphPart;
	public float tracksPerSecond;
	public float offSet; // length of wand
	public float triggerDistance; // minimal distance to current point that will trigger generating a new point

	// 1
	private SteamVR_TrackedObject trackedObj;
	// 2
	private SteamVR_Controller.Device Controller
	{
		get { return SteamVR_Controller.Input((int)trackedObj.index); }
	}


	private GameObject instantiatedEffect = null;
	private GameObject secondInstance = null;
	private GameObject thirdInstance = null;

	private bool tracking = false;
	//private GameObject glyphInstance = null;
	//private Vector3 startPos;

	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}

	// Update is called once per frame
	void Update () {
		// 3
		if (Controller.GetHairTriggerUp())
		{
			//engulfInFlames ();§
			// make the object diappear again
			//Destroy(glyphInstance);
			tracking = false; // will cause coroutine stop at next call
		}

		if (Controller.GetHairTriggerDown ()) {
			if (!tracking) {
				tracking = true;
				startNewGlyph();
				// now start the tracking coroutine
				StartCoroutine("trackGlyph");
//				startPos = trackedObj.transform.position;
//				glyphInstance = Instantiate (glyphPart);
			}


		}

		if (tracking) {
			//Vector3 currentPos = trackedObj.transform.position;
			//Vector3 middlePos = Vector3.Lerp (startPos, currentPos, 0.5f);
			//glyphInstance.transform.position = middlePos;
			//glyphInstance.transform.LookAt (currentPos);
			//glyphInstance.transform.localScale = new Vector3 (glyphInstance.transform.localScale.x, glyphInstance.transform.localScale.y, Vector3.Distance (startPos, currentPos));
		}

		// call samplePadFor gesture to start, complete and execute gesture recognition
		samplePadForGesture();

	}

	void Start () {
		// set up gesture recognizer
		initGesture();
	}

	void fireBolt () {
		Vector3 pos = trackedObj.transform.position;
		Quaternion rot = Quaternion.LookRotation (theTarget.transform.position - trackedObj.transform.position);
		if (instantiatedEffect) {
			Destroy(instantiatedEffect);
			instantiatedEffect = null;
		}
		instantiatedEffect = Instantiate(theEffect, pos, rot) as GameObject;
		instantiatedEffect.gameObject.AddComponent<RFX4_EffectSettingColor>().Color = Color.red;
	}


	void meteorStrike () {
		Vector3 pos = theTarget.transform.position;
		Quaternion rot = Quaternion.LookRotation (theTarget.transform.position - trackedObj.transform.position);
		if (secondInstance) {
			Destroy(secondInstance);
			secondInstance = null;
		}
		secondInstance = Instantiate(secondEffect, pos, rot) as GameObject;
		secondInstance.gameObject.AddComponent<RFX4_EffectSettingColor>().Color = Color.blue;
	}

	void engulfInFlames () {
		Vector3 pos = theTarget.transform.position;
		Quaternion rot = Quaternion.LookRotation (theTarget.transform.position - trackedObj.transform.position);
		if (thirdInstance) {
			Destroy(thirdInstance);
			thirdInstance = null;
		}
		thirdInstance = Instantiate(thirdEffect, pos, rot) as GameObject;
//		secondInstance.gameObject.AddComponent<RFX4_EffectSettingColor>().Color = Color.blue;
	}


	//
	// gesture recognzer vol 2: the real zing
	//
	private List<Vector3> positions = null; // the sampled positions
	private List<GameObject> parts = null; // the parts that make up the 

	void startNewGlyph() {
		if (positions == null) {
			positions = new List<Vector3> ();
		} else {
			positions.Clear ();
		}

		if (parts == null) {
			parts = new List<GameObject> ();
		} else {
			foreach (GameObject obj in parts) {
				Destroy (obj);
			}
			parts.Clear ();
		}
	}

	// add a segment between two pos
	void addSegment(Vector3 startPos, Vector3 currentPos) {
		GameObject glyphInstance = Instantiate (glyphPart);
		Vector3 middlePos = Vector3.Lerp (startPos, currentPos, 0.5f);
		glyphInstance.transform.position = middlePos;
		glyphInstance.transform.LookAt (currentPos);
		glyphInstance.transform.localScale = new Vector3 (glyphInstance.transform.localScale.x, glyphInstance.transform.localScale.y, Vector3.Distance (startPos, currentPos));
		parts.Add (glyphInstance);
	}


	// tracking co-routine
	IEnumerator trackGlyph () {
		while (tracking) {
			// get current point
			Vector3 currentPoint = trackedObj.transform.position;
			// we should now move it forward by offset. 
			currentPoint = currentPoint + offSet * trackedObj.transform.forward;
			int numPos = positions.Count;
			if (numPos < 1) {
				positions.Add (currentPoint);
			} else { 
				Vector3 lastPos = positions [numPos - 1];
				if (Vector3.Distance (lastPos, currentPoint) > triggerDistance) {
					// difference is large enough to warrant a new point
					positions.Add (currentPoint);
					addSegment (currentPoint, lastPos);
				}
				yield return new WaitForSeconds (1.0f / tracksPerSecond);
			}
		}

		// if we get here, the co-routine expires
		//return null;

		// we can now start evaluating the glyph

	}

	//
	// ================================
	//

	// stuff for getsture recognition
	private bool recording = false;
//	private bool gestureComplete = false;
	private string verticalBuffer = "";
	private string horizontalBuffer = "";
	private int lastUD = 0;
	private int lastLR = 0;
	//
	// gesture recognition stuff follows
	//
	void initGesture()
	{
		// call this when you are ready to start with a gesture
		recording = false;
//		gestureComplete = false;
		verticalBuffer = "";
		horizontalBuffer = "";
		lastUD = 0;
		lastLR = 0;

	}

	// 
	// interpret gesture. called when the finger is lifted, it runs though the
	// horizontal and vertial buffer patterns to match and find a gesture
	// 
	// it returns a string
	//

	string interpretGesture()
	{
		// see if it is a circle
		if ((verticalBuffer == "UDDU") && (horizontalBuffer == "RRLL")) return "Recognized: Circle 9";
		if ((verticalBuffer == "UDDUU") && (horizontalBuffer == "RRLL")) return "Recognized: Circle 9";
		if ((verticalBuffer == "DDUU") && (horizontalBuffer == "RLLR")) return "Recognized: Circle 12";


		if ((verticalBuffer == "") && (horizontalBuffer == "RR")) return "Recognized: Line Right";
		if ((verticalBuffer == "") && (horizontalBuffer == "LL")) return "Recognized: Line Left";
		if ((verticalBuffer == "") && (horizontalBuffer == "R")) return "Recognized: Line Right";
		if ((verticalBuffer == "") && (horizontalBuffer == "L")) return "Recognized: Line Left";

		if ((verticalBuffer == "UU") && (horizontalBuffer == "")) return "Recognized: Line Up";
		if ((verticalBuffer == "DD") && (horizontalBuffer == "")) return "Recognized: Line Down";
		if ((verticalBuffer == "U") && (horizontalBuffer == "")) return "Recognized: Line Up";
		if ((verticalBuffer == "D") && (horizontalBuffer == "")) return "Recognized: Line Down";

		if ((verticalBuffer == "DD") && (horizontalBuffer == "RL")) return "Recognized: Caret Right";
		if ((verticalBuffer == "DD") && (horizontalBuffer == "RRL")) return "Recognized: Caret Right";
		if ((verticalBuffer == "D") && (horizontalBuffer == "RRLL")) return "Recognized: Caret Right";
		if ((verticalBuffer == "D") && (horizontalBuffer == "RRL")) return "Recognized: Caret Right";

		if ((verticalBuffer == "DD") && (horizontalBuffer == "LR")) return "Recognized: Caret Left";
		if ((verticalBuffer == "DD") && (horizontalBuffer == "LLR")) return "Recognized: Caret Left";
		if ((verticalBuffer == "D") && (horizontalBuffer == "LLRR")) return "Recognized: Caret Left";
		if ((verticalBuffer == "D") && (horizontalBuffer == "LLR")) return "Recognized: Caret Left";


		return "*** trash:" + " HBuf:" + horizontalBuffer + " VBuf:" + verticalBuffer;
	}

	//
	// finished gesture is called when the finger is lifted
	// and the current gesture is complete.
	//
	// This is where you should first call interpretGesture and then 
	// do whatever you whink is apropriate
	//
	//
	void finishedGesture()
	{

		// see if we have recognized a gesture
		string theGesture = interpretGesture();

		// now look at the string ad do what is appropriate
		if (theGesture == "Recognized: Caret Right") {
			//laserUp = true;
			fireBolt();
		} else if (theGesture == "Recognized: Caret Left") {
			//laserUp = false;
		} else if (theGesture == "Recognized: Line Up") {
			//theFlame.GetComponent<ParticleSystem>().enableEmission = true;
			engulfInFlames ();
		} else if (theGesture == "Recognized: Line Down") {
			//theFlame.GetComponent<ParticleSystem>().enableEmission = false;
			meteorStrike();
		}
		else {
			// if we get here, no gesture was recognized or the recognized gesture was not used
			// for an action
			Debug.Log(gameObject.name + interpretGesture());

		}

	}

	//
	// SamplePadForGesture
	// Call during Update() is all that is required to run the gesture recognizer on this
	// controller
	//

	void samplePadForGesture()
	{
		Vector2 fingerLoc = Controller.GetAxis(); // read pad. if it is Vector2.zero, noone is touching
		// Vector2.zero is NOT (0,0) for some reason
		bool fingerDown = fingerLoc != Vector2.zero;

		if (!recording)
		{
			if (!fingerDown) return; // nothing to do
			// if we get here, we start a new glyph. lets init and then fall through to 
			// standard processing
			//gestureComplete = false;
			verticalBuffer = "";
			horizontalBuffer = "";
			lastUD = 0;
			lastLR = 0;
			recording = true;
			//Debug.Log(gameObject.name + " Gesture started!");
		}

		// when we get here, we are recording; on the first pass we drop
		// straight through from above, all inite to first pass
		if (!fingerDown)
		{
			// gesture has ended. Can't happen on first pass because there we know fingerdown is true
			recording = false;
			//gestureComplete = true;
			// we should probably now call a delegate
			finishedGesture();
			return;
		}

		int reducedH = 0;
		int reducedV = 0;
		// we now collapse the h and v axis from -1 .. 1 to an int 1, 2 or 3. we map the center 20% to 2, and 40% to left and right
		double h = fingerLoc.x + 1.0; // map from -1 .. 1 to 0..2
		double v = -fingerLoc.y + 1.0; // map the inverse, so 1 is on top, -1 is on bottom, them re-map from 0..2

		if (h <= 0.5) { reducedH = 1; } else if (h <= 1.5) { reducedH = 2; } else { reducedH = 3; }
		if (v <= 0.5) { reducedV = 1; } else if (v <= 1.5) { reducedV = 2; } else { reducedV = 3; }

		// But first we need to check if there is a value to compare to. If either lastLR is zero,
		// we simply store the value and exit
		if ((lastLR == 0) || (lastUD == 0))
		{
			lastLR = reducedH;
			lastUD = reducedV;
			return;
		}

		// we now check to see if this value is the same as the last value. If so, we can ignore
		// this because the hand hasn't moved and we are only interested in movement
		if ((reducedH != lastLR) || (reducedV != lastUD))
		{
			// something has changed, add to that string
			// we unroll the code in order to save cycles and not call a proc
			if (reducedH != lastLR)
			{
				// the finger has moved horizontally. see which way, and then generate a new string for the buffer
				if (reducedH > lastLR)
				{
					// finger has moved to the right. add a 'R' to the buffer
					horizontalBuffer = horizontalBuffer + "R";
					lastLR = lastLR + 1;
					if (reducedH > lastLR)
					{
						// if this was a quick move (jump from 1-->3), we may have to add another
						horizontalBuffer = horizontalBuffer + "R";
					}
				}
				else
				{
					// finger has moved left. reducedH is < lastLR
					horizontalBuffer = horizontalBuffer + "L";
					lastLR = lastLR - 1;
					if (reducedH < lastLR)
					{
						// if this was a quick move (jump from 1-->3), we may have to add another
						horizontalBuffer = horizontalBuffer + "L";
					}
				}
			}

			// now we do the same for vertical axis
			if (reducedV != lastUD)
			{
				// the finger has moved vertically. see which way, and then generate a new string for the buffer
				if (reducedV > lastUD)
				{
					// finger has moved down. add a 'D' to the buffer
					verticalBuffer = verticalBuffer + "D";
					lastUD = lastUD + 1;
					if (reducedV > lastUD)
					{
						// if this was a quick move (jump from 1-->3), we may have to add another
						verticalBuffer = verticalBuffer + "D";
					}
				}
				else
				{
					// finger has moved up. add a 'U' to the buffer
					verticalBuffer = verticalBuffer + "U";
					lastUD = lastUD - 1;
					if (reducedV < lastUD)
					{
						// if this was a quick move (jump from 1-->3), we may have to add another
						verticalBuffer = verticalBuffer + "K";
					}
				}

			}
			lastLR = reducedH;
			lastUD = reducedV;

		}
	}
}
