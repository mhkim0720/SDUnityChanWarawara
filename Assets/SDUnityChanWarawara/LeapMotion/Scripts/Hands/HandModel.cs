/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

namespace Leap.Unity{
  /**
  * The base class for all hand models, both graphics and physics.
  * 
  * This class serves as the interface between the HandController object
  * and the concrete hand object containing the graphics and physics of a hand.
  *
  * Subclasses of HandModel must implement InitHand() and UpdateHand(). The UpdateHand()
  * function is called in the Unity Update() phase for graphics HandModel instances;
  * and in the Unity FixedUpdate() phase for physics objects. InitHand() is called once,
  * when the hand is created and is followed by a call to UpdateHand().
  */
  public abstract class HandModel : IHandModel {
  
    [SerializeField]
    private Chirality handedness;
    public override Chirality Handedness {
      get { return handedness; }
    }
  
    private ModelType handModelType;
    public override abstract ModelType HandModelType {
      get;
    }
    
    /** The number of fingers on a hand.*/
    public const int NUM_FINGERS = 5;
  
    /** The model width of the hand in meters. This value is used with the measured value 
    * of the user's hand to scale the model proportionally.
    */
    public float handModelPalmWidth = 0.085f;
    /** The array of finger objects for this hand. The array is ordered from thumb (element 0) to pinky (element 4).*/
    public FingerModel[] fingers = new FingerModel[NUM_FINGERS];
  
    // Unity references
    /** Transform object for the palm object of this hand. */
    public Transform palm;
    /** Transform object for the forearm object of this hand. */
    public Transform forearm;
    /** Transform object for the wrist joint of this hand. */
    public Transform wristJoint;
    /** Transform object for the elbow joint of this hand. */
    public Transform elbowJoint;
    
    // Leap references
    /** The Leap Hand object this hand model represents. */
    protected Hand hand_;
    /** The parent HandController object for this hand. */
    //protected HandController controller_;
    protected LeapHandController controller_;
  
    /** 
    * Calculates the offset between the wrist position and the controller based
    * on the HandController.handMovementScale property and the Leap hand wrist position.
    */
    //TODO: Remove this
    public Vector3 GetHandOffset() {
      if (controller_ == null || hand_ == null)
        return Vector3.zero;
  
      Vector3 additional_movement = controller_.handMovementScale - Vector3.one;
  
      Vector3 scaled_wrist_position = Vector3.Scale(additional_movement, hand_.WristPosition.ToVector3());
  
      return controller_.transform.TransformPoint(scaled_wrist_position) -
             controller_.transform.position;
    }
  
    /** Calculates the position of the palm in global coordinates.
    * @returns A Vector3 containing the Unity coordinates of the palm position.
    */
    public Vector3 GetPalmPosition() {
      //if (hand_ != null) {
        return hand_.PalmPosition.ToVector3();
  
      //}
      //if (palm) {
      //  return palm.position;
      //}
      //return Vector3.zero;
    }
  
    /** Calculates the rotation of the hand in global coordinates.
    * @returns A Quaternion representing the rotation of the hand. 
    */
    public Quaternion GetPalmRotation() {
      if (hand_ != null) {
        return hand_.Basis.Rotation();
      }
      if (palm) {
        return palm.rotation;
      }
      return Quaternion.identity;
    }
  
    /** Calculates the direction vector of the hand in global coordinates.
    * @returns A Vector3 representing the direction of the hand.
    */
    public Vector3 GetPalmDirection() {
      if (hand_ != null) {
        return hand_.Direction.ToVector3();
      }
      if (palm) {
        return palm.forward;
      }
      return Vector3.forward;
    }
  
    /** Calculates the normal vector projecting from the hand in global coordinates.
    * @returns A Vector3 representing the vector perpendicular to the palm.
    */
    public Vector3 GetPalmNormal() {
      if (hand_ != null) {
        return hand_.PalmNormal.ToVector3();
      }
      if (palm) {
        return -palm.up;
      }
      return -Vector3.up;
    }
  
    /** Calculates the direction vector of the forearm in global coordinates.
    * @returns A Vector3 representing the direction of the forearm (pointing from elbow to wrist).
    */
    public Vector3 GetArmDirection() {
      if (hand_ != null) {
        return hand_.Arm.Direction.ToVector3();
      }
      if (forearm) {
        return forearm.forward;
      }
      return Vector3.forward;
    }
  
    /** Calculates the center of the forearm in global coordinates.
    * @returns A Vector3 containing the Unity coordinates of the center of the forearm.
    */
    public Vector3 GetArmCenter() {
      if (hand_ != null) {
        Vector leap_center = 0.5f * (hand_.Arm.WristPosition + hand_.Arm.ElbowPosition);
        return leap_center.ToVector3();
      }
      if (forearm) {
        return forearm.position;
      }
      return Vector3.zero;
    }
  
    /** Returns the measured length of the forearm in meters.*/
    public float GetArmLength() {
      return (hand_.Arm.WristPosition - hand_.Arm.ElbowPosition).Magnitude;
    }
    
    /** Returns the measured width of the forearm in meters.*/
    public float GetArmWidth() {
      return hand_.Arm.Width;
    }
  
    /** Calculates the position of the elbow in global coordinates.
    * @returns A Vector3 containing the Unity coordinates of the elbow.
    */
    public Vector3 GetElbowPosition() {
      if (hand_ != null) {
        Vector3 local_position = hand_.Arm.ElbowPosition.ToVector3();
        return local_position;
      }
      if (elbowJoint) {
        return elbowJoint.position;
      }
      return Vector3.zero;
    }
  
    /** Calculates the position of the wrist in global coordinates.
    * @returns A Vector3 containing the Unity coordinates of the wrist.
    */
    public Vector3 GetWristPosition() {
      if (hand_ != null) {
        Vector3 local_position = hand_.Arm.WristPosition.ToVector3();
        return local_position;
      }
      if (wristJoint) {
        return wristJoint.position;
      }
      return Vector3.zero;
    }
  
    /** Calculates the rotation of the forearm in global coordinates.
    * @returns A Quaternion representing the rotation of the arm. 
    */
    public Quaternion GetArmRotation() {
      if (hand_ != null) {
        Quaternion local_rotation = hand_.Arm.Basis.Rotation ();
        return local_rotation;
      }
      if (forearm) {
        return forearm.rotation;
      }
      return Quaternion.identity;
    }
  
    /** 
    * Returns the Leap Hand object represented by this HandModel.
    * Note that any physical quantities and directions obtained from the
    * Leap Hand object are relative to the Leap Motion coordinate system,
    * which uses a right-handed axes and units of millimeters.
    */
    public override Hand GetLeapHand() {
      return hand_;
    }
  
    /**
    * Assigns a Leap Hand object to this hand model.
    * Note that the Leap Hand objects are recreated every frame. The parent 
    * HandController calls this method to set or update the underlying hand.
    */
    public override void SetLeapHand(Hand hand) {
      hand_ = hand;
      for (int i = 0; i < fingers.Length; ++i) {
        if (fingers[i] != null) {
          fingers[i].SetLeapHand(hand_);
          fingers[i].SetOffset(GetHandOffset());
        }
      }
    }
  
  
  
  
    //Todo Kill these
    /** The parent HandController object of this hand.*/
    //public HandController GetController() {
    public LeapHandController GetController() {
      return controller_;
    }
   
    /** Sets the parent HandController object. */
    public void SetController(LeapHandController controller) {
      controller_ = controller;
      Debug.Log("SetController:" + controller_);
      for (int i = 0; i < fingers.Length; ++i) {
        if (fingers[i] != null)
          fingers[i].SetController(controller_);
      }
    }
  
    /** 
    * Implement this function to initialise this hand after it is created.
    * This function is called by the HandController during the Unity Update() phase when a new hand is detected
    * by the Leap Motion device.
    */
    public override void InitHand() {
      for (int f = 0; f < fingers.Length; ++f) {
        if (fingers[f] != null) {
          fingers[f].fingerType = (Finger.FingerType)f;
          fingers[f].InitFinger();
        }
      }
    }
  
    /**
     * Returns the ID associated with the hand in the Leap API.
     * This ID is guaranteed to be unique among all hands in a frame,
     * and is invariant for the lifetime of the hand model.
    */
    public int LeapID() {
      if (hand_ != null) {
        return hand_.Id;
      }
      return -1;
    }
  
    /** 
    * Implement this function to update this hand once every game loop.
    * For HandModel instances assigned to the HandController graphics hand list, the HandController calls this 
    * function during the Unity Update() phase. For HandModel instances in the physics hand list, the HandController
    * calls this function in the FixedUpdate() phase.
    */
    public override abstract void UpdateHand();
  }
}
