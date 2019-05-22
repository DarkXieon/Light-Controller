using LightControls.Controllers;

namespace LightControls.ControlOptions
{
    public abstract class InstancedControlOption
    {
        /// <summary>
        /// This method is used to check if a control option would be applied at a certian stage. 
        /// This method's main use is for the public light controller methods that allow enabling and disabling of controls based on what they are applying updates to.
        /// </summary>
        /// 
        /// <param name="stage">
        /// Stage to check
        /// </param>
        /// 
        /// <returns>
        /// Returns true if the stage will apply any changes at the given stage
        /// </returns>
        public virtual bool ApplyOn(ApplicationStages stage) { return false; }

        /// <summary>
        /// This method is used apply behaviour to controls to that advances the internal values and prepares the control to apply those values.
        /// This method does not get called by the light controller if UseControl is false HOWEVER the method does not check UseControl and calling it will still cause an update regardless to the value of UseControl.
        /// </summary>
        /// <returns>
        /// Returns true if the control has completed a single iteration
        /// </returns>
        public virtual bool UpdateControl() { return true; }

        /// <summary>
        /// This method is used to apply changes based on the control.
        /// This method is called once for each application stage there is.
        /// </summary>
        /// 
        /// <param name="controlInfo">
        /// The control info contains all of the lights and renderers that the current control group has in it, as well as the current application stage and the min and max intensity possible from an intensity control option if any are present.
        /// This method does not get called by the light controller if UseControl is false HOWEVER the method does not check UseControl and calling it will still cause the control to apply its values regardless to the value of UseControl.
        /// </param>
        public virtual void ApplyControl(ControlOptionGroup controlInfo) { }
    }
}
