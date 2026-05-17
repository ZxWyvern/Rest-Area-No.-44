using UnityEngine.InputSystem;

public partial class PlayerControls
{
    private InputActionMap m_Vehicle;
    private InputAction m_Vehicle_Throttle;
    private InputAction m_Vehicle_Steer;
    private InputAction m_Vehicle_ExitVehicle;

    private void InitVehicleMap()
    {
        if (m_Vehicle != null) return;

        m_Vehicle = asset.FindActionMap("Vehicle") ?? asset.AddActionMap("Vehicle");

        m_Vehicle_Throttle    = FindOrAdd(m_Vehicle, "Throttle",    InputActionType.Value);
        m_Vehicle_Steer       = FindOrAdd(m_Vehicle, "Steer",       InputActionType.Value);
        m_Vehicle_ExitVehicle = FindOrAdd(m_Vehicle, "ExitVehicle", InputActionType.Button);

        if (m_Vehicle_Throttle.bindings.Count == 0)
        {
            m_Vehicle_Throttle.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/s")
                .With("Positive", "<Keyboard>/w");
            m_Vehicle_Throttle.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/downArrow")
                .With("Positive", "<Keyboard>/upArrow");
            m_Vehicle_Throttle.AddCompositeBinding("1DAxis")
                .With("Negative", "<Gamepad>/leftTrigger")
                .With("Positive", "<Gamepad>/rightTrigger");
        }

        if (m_Vehicle_Steer.bindings.Count == 0)
        {
            m_Vehicle_Steer.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            m_Vehicle_Steer.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");
            m_Vehicle_Steer.AddBinding("<Gamepad>/leftStick/x");
        }

        if (m_Vehicle_ExitVehicle.bindings.Count == 0)
        {
            m_Vehicle_ExitVehicle.AddBinding("<Keyboard>/f");
            m_Vehicle_ExitVehicle.AddBinding("<Gamepad>/buttonEast");
        }
    }

    private static InputAction FindOrAdd(InputActionMap map, string name, InputActionType type)
    {
        return map.FindAction(name) ?? map.AddAction(name, type);
    }

    public VehicleActions Vehicle
    {
        get
        {
            InitVehicleMap();
            return new VehicleActions(this);
        }
    }

    public struct VehicleActions
    {
        private PlayerControls m_Wrapper;

        public VehicleActions(PlayerControls wrapper) { m_Wrapper = wrapper; }

        public InputAction Throttle    => m_Wrapper.m_Vehicle_Throttle;
        public InputAction Steer       => m_Wrapper.m_Vehicle_Steer;
        public InputAction ExitVehicle => m_Wrapper.m_Vehicle_ExitVehicle;

        public InputActionMap Get() { return m_Wrapper.m_Vehicle; }
        public void Enable()  { m_Wrapper.InitVehicleMap(); Get().Enable(); }
        public void Disable() { m_Wrapper.InitVehicleMap(); Get().Disable(); }
        public bool enabled   => m_Wrapper.m_Vehicle != null && Get().enabled;

        public void AddCallbacks(IVehicleActions instance)
        {
            if (instance == null) return;
            Throttle.started    += instance.OnThrottle;
            Throttle.performed  += instance.OnThrottle;
            Throttle.canceled   += instance.OnThrottle;
            Steer.started       += instance.OnSteer;
            Steer.performed     += instance.OnSteer;
            Steer.canceled      += instance.OnSteer;
            ExitVehicle.started    += instance.OnExitVehicle;
            ExitVehicle.performed  += instance.OnExitVehicle;
            ExitVehicle.canceled   += instance.OnExitVehicle;
        }

        public void RemoveCallbacks(IVehicleActions instance)
        {
            Throttle.started    -= instance.OnThrottle;
            Throttle.performed  -= instance.OnThrottle;
            Throttle.canceled   -= instance.OnThrottle;
            Steer.started       -= instance.OnSteer;
            Steer.performed     -= instance.OnSteer;
            Steer.canceled      -= instance.OnSteer;
            ExitVehicle.started    -= instance.OnExitVehicle;
            ExitVehicle.performed  -= instance.OnExitVehicle;
            ExitVehicle.canceled   -= instance.OnExitVehicle;
        }

        public void SetCallbacks(IVehicleActions instance)
        {
            Throttle.started    -= instance.OnThrottle;
            Throttle.performed  -= instance.OnThrottle;
            Throttle.canceled   -= instance.OnThrottle;
            Steer.started       -= instance.OnSteer;
            Steer.performed     -= instance.OnSteer;
            Steer.canceled      -= instance.OnSteer;
            ExitVehicle.started    -= instance.OnExitVehicle;
            ExitVehicle.performed  -= instance.OnExitVehicle;
            ExitVehicle.canceled   -= instance.OnExitVehicle;
            AddCallbacks(instance);
        }
    }

    public interface IVehicleActions
    {
        void OnThrottle(InputAction.CallbackContext context);
        void OnSteer(InputAction.CallbackContext context);
        void OnExitVehicle(InputAction.CallbackContext context);
    }
}
