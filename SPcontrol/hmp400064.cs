namespace InstrumentDrivers
{
    using System;
    using System.Runtime.InteropServices;


    public class hmp4000 : object, System.IDisposable
    {

        private System.Runtime.InteropServices.HandleRef _handle;

        private bool _disposed = true;

        ~hmp4000() { Dispose(false); }


        /// <summary>
        /// This function creates an IVI instrument driver session, typically using the C session instrument handle.
        /// </summary>
        /// <param name="Instrument_Handle">
        /// The instrument handle that is used to create an IVI instrument driver session.
        /// </param>
        public hmp4000(System.IntPtr Instrument_Handle)
        {
            this._handle = new System.Runtime.InteropServices.HandleRef(this, Instrument_Handle);
            this._disposed = false;
        }

        /// <summary>
        /// This function performs the following initialization actions:
        /// 
        /// - Creates a new instrument driver session.
        /// 
        /// - Opens a session to the specified device using the interface and address you specify for the Resource Name parameter.
        /// 
        /// - If the ID Query parameter is set to VI_TRUE, this function queries the instrument ID and checks that it is valid for this instrument driver.
        /// 
        /// - If the Reset parameter is set to VI_TRUE, this function resets the instrument to a known state.
        /// 
        /// - Sends initialization commands to set the instrument to the state necessary for the operation of the instrument driver.
        /// 
        /// - Returns a ViSession handle that you use to identify the instrument in all subsequent instrument driver function calls.
        /// 
        /// Note:
        /// 
        /// This function creates a new session each time you invoke it. Although you can open more than one session for the same resource, it is best not to do so. You can use the same session in multiple program threads.
        /// 
        /// </summary>
        /// <param name="Resource_Name">
        /// Pass the resource name of the device to initialize.
        /// 
        /// You can also pass the name of a virtual instrument or logical name that you configure with the VISA Configuration utility.  The virtual instrument identifies a specific device and specifies the initial settings for the session. A logical Name identifies a particular virtual instrument.
        /// 
        /// Refer to the following table below for the exact grammar to use for this parameter.  Optional fields are shown in square brackets ([]).
        /// 
        /// Interface   Grammar
        /// ------------------------------------------------------
        /// GPIB        GPIB[board]::primary address[::secondary address]
        ///             [::INSTR]
        /// VXI-11      TCPIP::remote_host::INSTR
        ///             
        /// The GPIB keyword is used for GPIB interface.
        /// The TCPIP keyword is used for VXI-11 interface.
        /// 
        /// Examples:
        /// (1) GPIB   - "GPIB::14::INSTR"
        /// (2) VXI-11 - "TCPIP::192.168.1.33::INSTR"
        /// 
        /// The default value for optional parameters are shown below.
        /// 
        /// Optional Parameter          Default Value
        /// -----------------------------------------
        /// board                       0
        /// secondary address           none - 31
        /// 
        /// </param>
        /// <param name="ID_Query">
        /// Specify whether you want the instrument driver to perform an ID Query.
        /// 
        /// Valid Range:
        /// VI_TRUE  (1) - Perform ID Query (Default Value)
        /// VI_FALSE (0) - Skip ID Query
        /// 
        /// When you set this parameter to VI_TRUE, the driver verifies that the instrument you initialize is a type that this driver supports.  
        /// 
        /// Circumstances can arise where it is undesirable to send an ID Query command string to the instrument.  When you set this parameter to VI_FALSE, the function initializes the instrument without performing an ID Query.
        /// </param>
        /// <param name="Reset_Device">
        /// Specify whether you want the to reset the instrument during the initialization procedure.
        /// 
        /// Valid Range:
        /// VI_TRUE  (1) - Reset Device (Default Value)
        /// VI_FALSE (0) - Don't Reset
        /// 
        /// 
        /// </param>
        public hmp4000(string Resource_Name, bool ID_Query, bool Reset_Device)
        {
            System.IntPtr instrumentHandle;
            int pInvokeResult = PInvoke.init(Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset_Device), out instrumentHandle);
            this._handle = new System.Runtime.InteropServices.HandleRef(this, instrumentHandle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// This function performs the following initialization actions:
        /// 
        /// - Creates a new instrument driver session and optionally sets the initial state of the following session attributes:
        /// 
        ///     HMP4000_ATTR_RANGE_CHECK
        ///     HMP4000_ATTR_QUERY_INSTRUMENT_STATUS
        ///     HMP4000_ATTR_CACHE (currently not used)
        ///     HMP4000_ATTR_SIMULATE
        ///     HMP4000_ATTR_RECORD_COERCIONS (currently not used)
        /// 
        /// - Opens a session to the specified device using the interface and address you specify for the Resource Name parameter.
        /// 
        /// - If the ID Query parameter is set to VI_TRUE, this function queries the instrument ID and checks that it is valid for this instrument driver.
        /// 
        /// - If the Reset parameter is set to VI_TRUE, this function resets the instrument to a known state.
        /// 
        /// - Sends initialization commands to set the instrument to the state necessary for the operation of the instrument driver.
        /// 
        /// - Returns a ViSession handle that you use to identify the instrument in all subsequent instrument driver function calls.
        /// 
        /// Note:
        /// 
        /// This function creates a new session each time you invoke it. Although you can open more than one session for the same resource, it is best not to do so. You can use the same session in multiple program threads.
        /// </summary>
        /// <param name="Resource_Name">
        /// Pass the resource name of the device to initialize.
        /// 
        /// You can also pass the name of a virtual instrument or logical name that you configure with the VISA Configuration utility.  The virtual instrument identifies a specific device and specifies the initial settings for the session. A logical Name identifies a particular virtual instrument.
        /// 
        /// Refer to the following table below for the exact grammar to use for this parameter.  Optional fields are shown in square brackets ([]).
        /// 
        /// Syntax
        /// ------------------------------------------------------
        /// GPIB[board]::&lt;primary address&gt;[::secondary address]::INSTR
        /// VXI[board]::&lt;logical address&gt;::INSTR
        /// GPIB-VXI[board]::&lt;logical address&gt;::INSTR
        /// ASRL&lt;port&gt;::INSTR
        /// &lt;LogicalName&gt;
        /// &lt;DriverSession&gt;
        /// 
        /// If you do not specify a value for an optional field, the following values are used:
        /// 
        /// Optional Field - Value
        /// ------------------------------------------------------
        /// board - 0
        /// secondary address - none (31)
        /// 
        /// The following table contains example valid values for this parameter.
        /// 
        /// "Valid Value" - Description
        /// ------------------------------------------------------
        /// "GPIB::22::INSTR" - GPIB board 0, primary address 22 no
        ///                     secondary address
        /// "GPIB::22::5::INSTR" - GPIB board 0, primary address 22
        ///                        secondary address 5
        /// "GPIB1::22::5::INSTR" - GPIB board 1, primary address 22
        ///                         secondary address 5
        /// "VXI::64::INSTR" - VXI board 0, logical address 64
        /// "VXI1::64::INSTR" - VXI board 1, logical address 64
        /// "GPIB-VXI::64::INSTR" - GPIB-VXI board 0, logical address 64
        /// "GPIB-VXI1::64::INSTR" - GPIB-VXI board 1, logical address 64
        /// "ASRL2::INSTR" - COM port 2
        /// "SampleInstr" - Logical name "SampleInstr"
        /// "xyz432" - Driver Session "xyz432"
        /// 
        /// Default Value: "GPIB::14::INSTR"
        /// 
        /// </param>
        /// <param name="ID_Query">
        /// Specify whether you want the instrument driver to perform an ID Query.
        /// 
        /// Valid Range:
        /// VI_TRUE  (1) - Perform ID Query (Default Value)
        /// VI_FALSE (0) - Skip ID Query
        /// 
        /// When you set this parameter to VI_TRUE, the driver verifies that the instrument you initialize is a type that this driver supports.  
        /// 
        /// Circumstances can arise where it is undesirable to send an ID Query command string to the instrument.  When you set this parameter to VI_FALSE, the function initializes the instrument without performing an ID Query.
        /// </param>
        /// <param name="Reset_Device">
        /// Specify whether you want the to reset the instrument during the initialization procedure.
        /// 
        /// Valid Range:
        /// VI_TRUE  (1) - Reset Device (Default Value)
        /// VI_FALSE (0) - Don't Reset
        /// 
        /// 
        /// </param>
        /// <param name="Option_String">
        /// You can use this control to set the initial value of certain attributes for the session.  The following table lists the attributes and the name you use in this parameter to identify the attribute.
        /// 
        /// Name              Attribute Defined Constant   
        /// --------------------------------------------
        /// RangeCheck        HMP4000_ATTR_RANGE_CHECK
        /// QueryInstrStatus  HMP4000_ATTR_QUERY_INSTRUMENT_STATUS   
        /// 
        /// The format of this string is, "AttributeName=Value" where AttributeName is the name of the attribute and Value is the value to which the attribute will be set.  To set multiple attributes, separate their assignments with a comma.  
        /// 
        /// If you pass NULL or an empty string for this parameter, the session uses the default values for the attributes.   You can override the default values by assigning a value explicitly in a string you pass for this parameter.  You do not have to specify all of the attributes and may leave any of them out.  If you do not specify one of the attributes, its default value will be used.  
        /// 
        /// The default values for the attributes are shown below:
        /// 
        ///     Attribute Name     Default Value
        ///     ----------------   -------------
        ///     RangeCheck         VI_TRUE
        ///     QueryInstrStatus   VI_TRUE
        ///     
        /// 
        /// The following are the valid values for ViBoolean attributes:
        /// 
        ///     True:     1, True, or VI_TRUE
        ///     False:    0, False, or VI_FALSE
        /// 
        /// 
        /// Default Value:
        ///        "QueryInstrStatus=1"
        /// 
        /// </param>
        public hmp4000(string Resource_Name, bool ID_Query, bool Reset_Device, string Option_String)
        {
            System.IntPtr instrumentHandle;
            int pInvokeResult = PInvoke.InitWithOptions(Resource_Name, System.Convert.ToUInt16(ID_Query), System.Convert.ToUInt16(Reset_Device), Option_String, out instrumentHandle);
            this._handle = new System.Runtime.InteropServices.HandleRef(this, instrumentHandle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            this._disposed = false;
        }

        /// <summary>
        /// Gets the instrument handle.
        /// </summary>
        /// <value>
        /// The value is the IntPtr that represents the handle to the instrument.
        /// </value>
        public System.IntPtr Handle
        {
            get
            {
                return this._handle.Handle;
            }
        }

        /// <summary>
        /// This function selects the channels. All the following channel-selective commands will be applied for this channel
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SELECTION_OF_CHANNEL
        /// 
        /// Remote-control command(s):
        /// INSTrument:SELect OUTPut1 | OUTPut2 | OUTPut3 | OUTPut4
        /// </summary>
        /// <param name="Channel">
        /// This control selects the output channel.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_OUTPUT_1 (1) - Channel 1
        /// HMP4000_VAL_OUTPUT_2 (2) - Channel 2
        /// HMP4000_VAL_OUTPUT_3 (3) - Channel 3
        /// HMP4000_VAL_OUTPUT_4 (4) - Channel 4
        /// 
        /// Default Value: HMP4000_VAL_OUTPUT_1 (1)
        /// 
        /// Attribute:
        /// HMP4000_ATTR_SELECTION_OF_CHANNEL
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureChannel(int Channel)
        {
            int pInvokeResult = PInvoke.ConfigureChannel(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the currently selected channel.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SELECTION_OF_CHANNEL
        /// 
        /// Remote-control command(s):
        /// INSTrument:SELect?
        /// </summary>
        /// <param name="Channel">
        /// This control returns the selected channel.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_OUTPUT_1 (1) - Channel 1
        /// HMP4000_VAL_OUTPUT_2 (2) - Channel 2
        /// HMP4000_VAL_OUTPUT_3 (3) - Channel 3
        /// HMP4000_VAL_OUTPUT_4 (4) - Channel 4
        /// 
        /// Attribute:
        /// HMP4000_ATTR_SELECTION_OF_CHANNEL
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int GetChannel(out int Channel)
        {
            int pInvokeResult = PInvoke.GetChannel(this._handle, out Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures voltage and current. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_VOLTAGE_LEVEL
        /// HMP4000_ATTR_OUTPUT_CURRENT_LEVEL
        /// 
        /// Remote-control command(s):
        /// SOURce:VOLTage:LEVel:AMPLitude
        /// SOURce:CURRent:LEVel:AMPLitude
        /// 
        /// </summary>
        /// <param name="Voltage">
        /// This control adjust the output voltage level. 
        /// 
        /// Valid Values:
        /// 0.0 to 32.0 V
        /// 
        /// Default Value:
        /// 0.0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_VOLTAGE_LEVEL
        /// 
        /// Note(s):
        /// 
        /// (1) - maximum level depends on channel
        /// 
        /// </param>
        /// <param name="Current">
        /// This control adjust the output current value. 
        /// 
        /// Valid Values:
        /// 0.0 to 10.0 A
        /// 
        /// Default Value:
        /// 0.0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_CURRENT_LEVEL
        /// 
        /// Note(s):
        /// 
        /// (1) - maximum value depends on channel
        /// 
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureCombinedVoltageAndCurrent(double Voltage, double Current)
        {
            int pInvokeResult = PInvoke.ConfigureCombinedVoltageAndCurrent(this._handle, Voltage, Current);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Turns the channel output ON or OFF. When switching the channel ON, General output switch is also switched ON. When switching the channel OFF, General output switch is not changed. If changing the General output switch status is not desired, use the hmp4000_ConfigureOutputStateChannelOnly.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_STATE
        /// 
        /// Remote-control command(s):
        /// OUTPut:STATe ON | OFF
        /// </summary>
        /// <param name="Output_State">
        /// Switches the output On or Off.
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On
        /// 
        /// Default Value: VI_FALSE (0)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_STATE
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsngpq_error_message function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF     VISA     Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF     VISA     Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int ConfigureOutputState(bool Output_State)
        {
            int pInvokeResult = PInvoke.ConfigureOutputState(this._handle, System.Convert.ToUInt16(Output_State));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Turning on / off all previous selected channels simultaneously.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_GENERAL_OUTPUT_STATE
        /// 
        /// Remote-control command(s):
        /// OUTPut:GENeral
        /// </summary>
        /// <param name="Output_State">
        /// Turning on / off all previous selected channels simultaneously. 
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On
        /// 
        /// Default Value: VI_FALSE (0)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_GENERAL_OUTPUT_STATE
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsngpq_error_message function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF     VISA     Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF     VISA     Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int ConfigureGeneralOutputState(bool Output_State)
        {
            int pInvokeResult = PInvoke.ConfigureGeneralOutputState(this._handle, System.Convert.ToUInt16(Output_State));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Turns the channel output ON or OFF. Compared to hmp4000_ConfigureOutputState, it doesn't change the General output switch settings. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_SELECT
        /// 
        /// Remote-control command(s):
        /// OUTPut:SELect
        /// </summary>
        /// <param name="Output_State">
        /// Turns the channel output ON or OFF.
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On
        /// 
        /// Default Value: VI_FALSE (0)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_SELECT
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsngpq_error_message function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF     VISA     Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF     VISA     Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int ConfigureOutputStateChannelOnly(bool Output_State)
        {
            int pInvokeResult = PInvoke.ConfigureOutputStateChannelOnly(this._handle, System.Convert.ToUInt16(Output_State));
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function stores or recalls the settings from selected locations of a nonvolatile memory.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SETTINGS_STORE_LOCATION
        /// HMP4000_ATTR_SETTINGS_RECALL_LOCATION
        /// 
        /// Remote-control command(s):
        /// *SAV {0|1|2|3|4|5|6|7|8|9}
        /// *RCL {0|1|2|3|4|5|6|7|8|9}
        /// </summary>
        /// <param name="Memory_Operation">
        /// This control selects the memory operation.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_MEM_OPERATION_STORE  (0) - Store
        /// HMP4000_VAL_MEM_OPERATION_RECALL (1) - Recall
        /// 
        /// Default Value: HMP4000_VAL_MEM_OPERATION_STORE  (0)
        /// </param>
        /// <param name="Location">
        /// This control selects the location. 
        /// 
        /// Valid Values:
        /// 0 to 9
        /// 
        /// Default Value:
        /// 0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SETTINGS_STORE_LOCATION
        /// HMP4000_ATTR_SETTINGS_RECALL_LOCATION
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureSettingsLocation(int Memory_Operation, int Location)
        {
            int pInvokeResult = PInvoke.ConfigureSettingsLocation(this._handle, Memory_Operation, Location);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries combined setting of voltage and current. 
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// Remote-control command(s):
        /// APPLy?
        /// </summary>
        /// <param name="Voltage_Value">
        /// This control returns the output voltage level. 
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// </param>
        /// <param name="Current">
        /// This control returns the output current value. 
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int QueryCombinedVoltageAndCurrent(out double Voltage_Value, out double Current)
        {
            int pInvokeResult = PInvoke.QueryCombinedVoltageAndCurrent(this._handle, out Voltage_Value, out Current);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function sets the parameters of the freely programmable waveforms. Set points defining voltage, current and dwell
        /// time are required.
        /// 
        /// A maximum of 128 set points (index 0 ... 128) may be used and
        /// will be repetitively addressed.
        /// 
        /// The maximum number of repetitions is 255. If Repetitons 0 is selected, the waveform will be repeated indefinitely. 
        /// 
        /// Attribute(s):
        /// -
        /// HMP4000_ATTR_ARBITRARY_GENERTOR_REPETITION
        /// 
        /// Remote-control command(s):
        /// ARBitrary:DATA &lt;voltage1, current1, time1, voltage2, ...&gt;
        /// ARBitrary:REPetitions
        /// </summary>
        /// <param name="Array_Size">
        /// This control defines no. of points of voltage, current and time array for Waveform Data. 
        /// 
        /// Valid Values:
        /// 0 to 128
        /// 
        /// Default Value: 128
        /// 
        /// </param>
        /// <param name="Voltage">
        /// This control defines the array of voltage points for waveform data.
        /// 
        /// Valid Values:
        /// 0.0 to 32.0 V
        /// 
        /// Default Value: 0.0
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// </param>
        /// <param name="Current">
        /// This control defines the array of current points for waveform data.
        /// 
        /// Valid Values:
        /// 0.0 to 10.0 A
        /// 
        /// Default Value: 0.0
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// </param>
        /// <param name="Time">
        /// This control defines the array of time points for waveform data.
        /// 
        /// Valid Values:
        /// 0.0 to 99.99 s
        /// 
        /// Default Value: 0.0
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// </param>
        /// <param name="Repetition">
        /// No. of repetition. The maximum number of repetitions is 255. If 0 is selected, the waveform will be repeated indefi nitely.
        /// 
        /// Valid Values:
        /// 0 to 255
        /// 
        /// Default Value: 0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERTOR_REPETITION
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureArbitraryGeneratorWaveformData(int Array_Size, double[] Voltage, double[] Current, double[] Time, int Repetition)
        {
            int pInvokeResult = PInvoke.ConfigureArbitraryGeneratorWaveformData(this._handle, Array_Size, Voltage, Current, Time, Repetition);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects the channel for starting arbitrary generator. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERTOR_START_CHANNEL
        /// 
        /// Remote-control command(s):
        /// ARBitrary:STARt
        /// </summary>
        /// <param name="Channel">
        /// This control selects the output channel.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_CH_1 (1) - Output Channel 1
        /// HMP4000_VAL_CH_2 (2) - Output Channel 2
        /// HMP4000_VAL_CH_3 (3) - Output Channel 3
        /// HMP4000_VAL_CH_4 (4) - Output Channel 4
        /// 
        /// Default Value: HMP4000_VAL_CH_1 (1)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERTOR_START_CHANNEL
        /// 
        /// Note(s):
        /// HMP4030 - Channel 4 not available
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureArbitraryGeneratorStartChannel(int Channel)
        {
            int pInvokeResult = PInvoke.ConfigureArbitraryGeneratorStartChannel(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects the channel where stop the arbitrary generator. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERTOR_STOP_CHANNEL
        /// 
        /// Remote-control command(s):
        /// ARBitrary:STOP
        /// </summary>
        /// <param name="Channel">
        /// This control selects the output channel.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_CH_1 (1) - Output Channel 1
        /// HMP4000_VAL_CH_2 (2) - Output Channel 2
        /// HMP4000_VAL_CH_3 (3) - Output Channel 3
        /// HMP4000_VAL_CH_4 (4) - Output Channel 4
        /// 
        /// Default Value: HMP4000_VAL_CH_1 (1)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERTOR_STOP_CHANNEL
        /// 
        /// Note(s):
        /// HMP4030 - Channel 4 not available
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureArbitraryGeneratorStopChannel(int Channel)
        {
            int pInvokeResult = PInvoke.ConfigureArbitraryGeneratorStopChannel(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects the channel where the data entered are sent to.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_DATA_TRANSFER_CHANNEL
        /// 
        /// Remote-control command(s):
        /// ARBitrary:TRANsfer
        /// </summary>
        /// <param name="Channel">
        /// This control selects the output channel.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_CH_1 (1) - Output Channel 1
        /// HMP4000_VAL_CH_2 (2) - Output Channel 2
        /// HMP4000_VAL_CH_3 (3) - Output Channel 3
        /// HMP4000_VAL_CH_4 (4) - Output Channel 4
        /// 
        /// Default Value: HMP4000_VAL_CH_1 (1)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_DATA_TRANSFER_CHANNEL
        /// 
        /// Note(s):
        /// HMP4030 - Channel 4 not available
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureArbitraryGeneratorDataTransferChannel(int Channel)
        {
            int pInvokeResult = PInvoke.ConfigureArbitraryGeneratorDataTransferChannel(this._handle, Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function selects the storage for saving, recalling or clearing arbitrary generator waveform data. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_SAVE
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_RECALL
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_CLEAR
        /// 
        /// Remote-control command(s):
        /// ARBitrary:SAVE
        /// ARBitrary:RESTore
        /// ARBitrary:CLEar
        /// 
        /// </summary>
        /// <param name="Memory_Operation">
        /// This control selects the memory operation.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_MEM_OPERATION_STORE  (0) - Store
        /// HMP4000_VAL_MEM_OPERATION_RECALL (1) - Recall
        /// HMP4000_VAL_MEM_OPERATION_CLEAR  (2) - Clear
        /// 
        /// Default Value: HMP4000_VAL_MEM_OPERATION_STORE  (0)
        /// </param>
        /// <param name="Memory_Index">
        /// This control selects the memory index.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_MEM_1 (1) - Memory index 1
        /// HMP4000_VAL_MEM_2 (2) - Memory index 2
        /// HMP4000_VAL_MEM_3 (3) - Memory index 3
        /// 
        /// Default Value: HMP4000_VAL_MEM_1 (1)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_SAVE
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_RECALL
        /// HMP4000_ATTR_ARBITRARY_GENERATOR_CLEAR
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureArbitraryGeneratorDataStorage(int Memory_Operation, int Memory_Index)
        {
            int pInvokeResult = PInvoke.ConfigureArbitraryGeneratorDataStorage(this._handle, Memory_Operation, Memory_Index);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function adjusts the output current. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_CURRENT_LEVEL
        /// 
        /// Remote-control command(s):
        /// SOURce:CURRent:LEVel:AMPLitude
        /// 
        /// </summary>
        /// <param name="Current">
        /// This control adjust the output current value. 
        /// 
        /// Valid Values:
        /// 0.0 to 10.0 A
        /// 
        /// Default Value:
        /// 0.0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_CURRENT_LEVEL
        /// 
        /// Note(s):
        /// 
        /// (1) - maximum value depends on channel
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureCurrentValue(double Current)
        {
            int pInvokeResult = PInvoke.ConfigureCurrentValue(this._handle, Current);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function switches On or Off the electronic fuse and link or unlink from the channel.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ELECTRONIC_FUSE_STATE
        /// HMP4000_ATTR_ELECTRONIC_FUSE_CHANNEL_LINK
        /// HMP4000_ATTR_ELECTRONIC_FUSE_CHANNEL_UNLINK
        /// 
        /// Remote-control command(s):
        /// FUSE:STATe ON | OFF
        /// FUSE:LINK
        /// FUSE:UNLink
        /// </summary>
        /// <param name="Fuse">
        /// Switches the fuse On or Off.
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On
        /// 
        /// Default Value: VI_FALSE (0)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ELECTRONIC_FUSE_STATE
        /// 
        /// </param>
        /// <param name="Link_to_Channel">
        /// This control selects the output channel for electronic fuse.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_CH_N (0)  - Not Used
        /// HMP4000_VAL_CH_1 (1)  - Output Channel 1
        /// HMP4000_VAL_CH_2 (2)  - Output Channel 2
        /// HMP4000_VAL_CH_3 (3)  - Output Channel 3
        /// HMP4000_VAL_CH_4 (4)  - Output Channel 4
        /// 
        /// Default Value: HMP4000_VAL_CH_N (0)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ELECTRONIC_FUSE_CHANNEL_LINK
        /// 
        /// Note(s):
        /// HMP4030 - Channel 4 not available
        /// 
        /// </param>
        /// <param name="Unlink_from_Channel">
        /// This control selects the output channel for electronic fuse.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_CH_N (0) - Not Used
        /// HMP4000_VAL_CH_1 (1) - Output Channel 1
        /// HMP4000_VAL_CH_2 (2) - Output Channel 2
        /// HMP4000_VAL_CH_3 (3) - Output Channel 3
        /// HMP4000_VAL_CH_4 (4) - Output Channel 4
        /// 
        /// Default Value: HMP4000_VAL_CH_N (0)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_ELECTRONIC_FUSE_CHANNEL_UNLINK
        /// 
        /// Note(s):
        /// HMP4030 - Channel 4 not available
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsngpq_error_message function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF     VISA     Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF     VISA     Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int ConfigureElectronicFuse(bool Fuse, int Link_to_Channel, int Unlink_from_Channel)
        {
            int pInvokeResult = PInvoke.ConfigureElectronicFuse(this._handle, System.Convert.ToUInt16(Fuse), Link_to_Channel, Unlink_from_Channel);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the output current step value. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_CURRENT_LEVEL_STEP
        /// 
        /// Remote-control command(s):
        /// SOURce:CURRent:LEVel:AMPLitude:STEP:INCRement
        /// </summary>
        /// <param name="Current_Step">
        /// This control sets the current step.
        /// 
        /// Valid Values:
        /// 0.0 to 10.0 A
        /// 
        /// Default Value: 0.0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_CURRENT_LEVEL_STEP
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureCurrentStepValue(double Current_Step)
        {
            int pInvokeResult = PInvoke.ConfigureCurrentStepValue(this._handle, Current_Step);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the electronic fuse status. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUERY_ELECTRONIC_FUSE_STATUS
        /// 
        /// Remote-control command(s):
        /// FUSE:TRIPped?
        /// </summary>
        /// <param name="Fuse_Status">
        /// Returns the fuse status.
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUERY_ELECTRONIC_FUSE_STATUS
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int QueryElectronicFuseStatus(out bool Fuse_Status)
        {
            ushort Fuse_StatusAsUShort;
            int pInvokeResult = PInvoke.QueryElectronicFuseStatus(this._handle, out Fuse_StatusAsUShort);
            Fuse_Status = System.Convert.ToBoolean(Fuse_StatusAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function adjusts the output voltage level. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_VOLTAGE_LEVEL
        /// 
        /// Remote-control command(s):
        /// SOURce:VOLTage:LEVel:AMPLitude
        /// 
        /// </summary>
        /// <param name="Voltage">
        /// This control adjust the output voltage level. 
        /// 
        /// Valid Values:
        /// 0.0 to 32.0 V
        /// 
        /// Default Value:
        /// 0.0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_VOLTAGE_LEVEL
        /// 
        /// Note(s):
        /// 
        /// (1) - maximum level depends on channel
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureVoltageValue(double Voltage)
        {
            int pInvokeResult = PInvoke.ConfigureVoltageValue(this._handle, Voltage);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the over voltage protection (OVP). 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OVER_VOLTAGE_PROTECTION_LEVEL
        /// 
        /// Remote-control command(s):
        /// VOLTage:PROTection:LEVel
        /// 
        /// </summary>
        /// <param name="OVP_Value">
        /// This control configures the output over voltage protection level. 
        /// 
        /// Valid Values:
        /// 0.0 to 33.0 V
        /// 
        /// Default Value:
        /// 33.0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OVER_VOLTAGE_PROTECTION_LEVEL
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureOverVoltageProtection(double OVP_Value)
        {
            int pInvokeResult = PInvoke.ConfigureOverVoltageProtection(this._handle, OVP_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the output voltage step value. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_VOLTAGE_LEVEL_STEP
        /// 
        /// Remote-control command(s):
        /// SOURce:VOLTage:LEVel:AMPLitude:STEP:INCRement
        /// </summary>
        /// <param name="Voltage_Step">
        /// This control sets the voltge step.
        /// 
        /// Valid Values:
        /// 0.0 to 32.0 V
        /// 
        /// Default Value: 0.0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OUTPUT_VOLTAGE_LEVEL_STEP
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureVoltageStepValue(double Voltage_Step)
        {
            int pInvokeResult = PInvoke.ConfigureVoltageStepValue(this._handle, Voltage_Step);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the over voltage protection (OVP) status. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUERY_OVER_VOLTAGE_PROTECTION_STATUS
        /// 
        /// Remote-control command(s):
        /// VOLTage:PROTection:TRIPped?
        /// </summary>
        /// <param name="OVP_Status">
        /// Returns the OVP status.
        /// 
        /// Valid Values:
        /// VI_FALSE (0) - Off
        /// VI_TRUE  (1) - On
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUERY_OVER_VOLTAGE_PROTECTION_STATUS
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int QueryOverVoltageProtectionStatus(out bool OVP_Status)
        {
            ushort OVP_StatusAsUShort;
            int pInvokeResult = PInvoke.QueryOverVoltageProtectionStatus(this._handle, out OVP_StatusAsUShort);
            OVP_Status = System.Convert.ToBoolean(OVP_StatusAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function clears the over voltage protection (OVP). 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_OVER_VOLTAGE_PROTECTION_CLEAR
        /// 
        /// Remote-control command(s):
        /// VOLTage:PROTection:CLEar
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ClearOverVoltageProtection()
        {
            int pInvokeResult = PInvoke.ClearOverVoltageProtection(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the beeper.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_BEEPER_MODE
        /// 
        /// Remote-control command(s):
        /// SYSTem:BEEPer[:IMMediate]
        /// </summary>
        /// <param name="Beeper">
        /// This control selects the mode of the beeper.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_BEEPER_OFF      (0) - Off
        /// HMP4000_VAL_BEEPER_ON       (1) - On
        /// HMP4000_VAL_BEEPER_CRITICAL (2) - Critical Events Only
        /// 
        /// Default Value: HMP4000_VAL_BEEPER_OFF (0)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_BEEPER_MODE
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ConfigureBeeper(int Beeper)
        {
            int pInvokeResult = PInvoke.ConfigureBeeper(this._handle, Beeper);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the measured voltage value. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_MEASUREMENT_DC_VOLTAGE
        /// 
        /// Remote-control command(s):
        /// MEASure[:VOLTage][:DC]?
        /// </summary>
        /// <param name="Voltage_Value">
        /// This control returns the measured value.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_MEASUREMENT_DC_VOLTAGE
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ReadVoltageOutput(out double Voltage_Value)
        {
            int pInvokeResult = PInvoke.ReadVoltageOutput(this._handle, out Voltage_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the measured Current value. 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_MEASUREMENT_DC_CURRENT
        /// 
        /// Remote-control command(s):
        /// MEASure:CURRent[:DC]?
        /// </summary>
        /// <param name="Current_Value">
        /// This control returns the measured value.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_MEASUREMENT_DC_CURRENT
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ReadCurrentOutput(out double Current_Value)
        {
            int pInvokeResult = PInvoke.ReadCurrentOutput(this._handle, out Current_Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function resets the instrument to a known state and sends initialization commands to the instrument. The initialization commands set instrument settings such as Headers Off, Short Command form, and Data Transfer Binary to the state necessary for the operation of the instrument driver.
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// Remote-control command(s):
        /// *RST
        /// 
        /// 
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int reset()
        {
            int pInvokeResult = PInvoke.reset(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function runs the instrument's self test routine and returns the test result(s). 
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SERVICE_STEST
        /// 
        /// Remote-control command(s):
        /// *TST?
        /// 
        /// </summary>
        /// <param name="Self_Test_Result">
        /// This control contains the value returned from the instrument self test.  Zero means success.  For any other code, see the device's operator's manual.
        /// 
        /// </param>
        /// <param name="Self_Test_Message">
        /// This control contains the string returned from the self test. See the device's operation manual for an explanation of the string's contents.
        /// 
        /// Notes:
        /// 
        /// (1) The array must contain at least 256 elements ViChar[256].
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int self_test(out short Self_Test_Result, System.Text.StringBuilder Self_Test_Message)
        {
            int pInvokeResult = PInvoke.self_test(this._handle, out Self_Test_Result, Self_Test_Message);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the revision numbers of the instrument driver and instrument firmware.
        /// 
        /// Attribute(s):
        /// 
        /// Remote-control command(s):
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="Instrument_Driver_Revision">
        /// Returns the instrument driver software revision numbers in the form of a string.
        /// 
        /// You must pass a ViChar array with at least 256 bytes.
        /// </param>
        /// <param name="Firmware_Revision">
        /// Returns the instrument firmware revision numbers in the form of a string.
        /// 
        /// You must pass a ViChar array with at least 256 bytes.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int revision_query(System.Text.StringBuilder Instrument_Driver_Revision, System.Text.StringBuilder Firmware_Revision)
        {
            int pInvokeResult = PInvoke.revision_query(this._handle, Instrument_Driver_Revision, Firmware_Revision);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the number of the SCPI version, which is relevant for the instrument.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SYSTEM_VERSION
        /// 
        /// Remote-control command(s):
        /// SYSTem:VERSion?
        /// </summary>
        /// <param name="Length">
        /// Sets the allocated memory length of System Version string.
        /// 
        /// Valid Values: &gt;0
        /// 
        /// Default Value: 255
        /// </param>
        /// <param name="System_Version">
        /// Returns the number of the SCPI version, which is relevant for the instrument.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the agesa_error_message function.  To obtain additional information about the error condition, call the agesa_GetErrorInfo function.  To clear the error information from the driver, call the agesa_ClearErrorInfo function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state when the
        ///           measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        /// BFFA4001  The Active Marker is not a span marker.
        /// BFFA4002  The Active Marker is not a band marker.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code  Types    
        /// -------------------------------------------------
        /// 3FFA2000 to 3FFA3FFF     IviSpecAn    Warnings
        /// 3FFA0000 to 3FFA1FFF     IVI          Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA         Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP       Driver Warnings
        ///           
        /// BFFA2000 to BFFA3FFF     IviSpecAn    Errors
        /// BFFA0000 to BFFA1FFF     IVI          Errors
        /// BFFF0000 to BFFFFFFF     VISA         Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP       Driver Errors
        /// </returns>
        public int SystemVersion(int Length, System.Text.StringBuilder System_Version)
        {
            int pInvokeResult = PInvoke.SystemVersion(this._handle, Length, System_Version);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads an error code and a message from the instrument's error queue.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SYST_ERR
        /// 
        /// Remote-control command(s):
        /// SYSTem:ERRor?
        /// </summary>
        /// <param name="Error_Code">
        /// Returns the error code read from the instrument's error queue.
        /// 
        /// 
        /// </param>
        /// <param name="Error_Message">
        /// Returns the error message string read from the instrument's error message queue.
        /// 
        /// You must pass a ViChar array with at least 1024 bytes.
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int error_query(out int Error_Code, System.Text.StringBuilder Error_Message)
        {
            int pInvokeResult = PInvoke.error_query(this._handle, out Error_Code, Error_Message);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the selected register enable values.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUESTIONABLE_ENABLE_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_ENABLE_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_ENABLE_REGISTER
        /// 
        /// Remote-control command(s):
        /// STATus:QUEStionable:ENABle
        /// STATus:QUEStionable:INSTrument:ENABle
        /// STATus:QUEStionable:INSTrument:ISUMmary&lt;n&gt;:ENABle
        /// </summary>
        /// <param name="Registers">
        /// Select register.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_QUESTIONABLE_REGISTER (0) - Questionable Register
        /// HMP4000_VAL_QUESTIONABLE_INSTRUMENT_REGISTER (1) - Questionable Instrument Register
        /// HMP4000_VAL_QUESTIONABLE_INSTRUMENT_SPECIFIC_REGISTER (2) - Questionable Instrument Specific Register
        /// 
        /// Default Value: HMP4000_VAL_QUESTIONABLE_REGISTER (0)
        /// </param>
        /// <param name="Channel">
        /// This control selects the channel.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_CH_1 (1) - Output Channel 1
        /// HMP4000_VAL_CH_2 (2) - Output Channel 2
        /// HMP4000_VAL_CH_3 (3) - Output Channel 3
        /// HMP4000_VAL_CH_4 (4) - Output Channel 4
        /// 
        /// Default Value: HMP4000_VAL_CH_1 (1)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_ENABLE_REGISTER
        /// 
        /// Note(s):
        /// (1) HMP4030 - Channel 4 not available
        /// (2) Valid only for instrument specific registers
        /// </param>
        /// <param name="Value">
        /// Configures the selected register value.
        /// 
        /// Default Value: 0
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUESTIONABLE_ENABLE_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_ENABLE_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_ENABLE_REGISTER
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsngpt_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// BFFC0002  Parameter 2 (Registers) out of range.
        /// BFFC0003  Parameter 3 (Return Value) NULL pointer.
        /// 
        /// BFFC09F0  Instrument status error.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF     VISA     Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFF0000 to BFFFFFFF     VISA     Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int SetRegister(int Registers, int Channel, int Value)
        {
            int pInvokeResult = PInvoke.SetRegister(this._handle, Registers, Channel, Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the selected register event and condition values.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUESTIONABLE_EVENT_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_EVENT_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_EVENT_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_CONDITION_REGISTER
        /// 
        /// Remote-control command(s):
        /// STATus:QUEStionable[:EVENt]?
        /// STATus:QUEStionable:INSTrument[:EVENt]?
        /// STATus:QUEStionable:INSTrument:ISUMmary&lt;n&gt;[:EVENt]?
        /// STATus:QUEStionable:INSTrument:ISUMmary&lt;n&gt;:CONDition?
        /// 
        /// </summary>
        /// <param name="Registers">
        /// Select register.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_QUESTIONABLE_REGISTER (0) - Questionable Register
        /// HMP4000_VAL_QUESTIONABLE_INSTRUMENT_REGISTER (1) - Questionable Instrument Register
        /// HMP4000_VAL_QUESTIONABLE_INSTRUMENT_SPECIFIC_REGISTER (2) - Questionable Instrument Specific Register
        /// HMP4000_VAL_QUESTIONABLE_INSTRUMENT_SPECIFIC_CONDITION_REGISTER (3) - Questionable Instrument Specific Register Condition
        /// 
        /// Default Value: HMP4000_VAL_QUESTIONABLE_REGISTER (0)
        /// </param>
        /// <param name="Channel">
        /// This control selects the channel.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_CH_1 (1) - Output Channel 1
        /// HMP4000_VAL_CH_2 (2) - Output Channel 2
        /// HMP4000_VAL_CH_3 (3) - Output Channel 3
        /// HMP4000_VAL_CH_4 (4) - Output Channel 4
        /// 
        /// Default Value: HMP4000_VAL_CH_1 (1)
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_EVENT_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_CONDITION_REGISTER
        /// 
        /// Note(s):
        /// (1) HMP4030 - Channel 4 not available
        /// (2) Valid only for instrument specific registers
        /// </param>
        /// <param name="Value">
        /// Returns the selected register value.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_QUESTIONABLE_EVENT_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_EVENT_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_EVENT_REGISTER
        /// HMP4000_ATTR_QUESTIONABLE_INSTRUMENT_SPECIFIC_CONDITION_REGISTER
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation. The status code  either indicates success or describes an error or warning condition. You examine the status code from each call to an instrument driver function to determine if an error occurred. To obtain a text description of the status code, call the rsngpt_error_message function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// BFFC0002  Parameter 2 (Registers) out of range.
        /// BFFC0003  Parameter 3 (Return Value) NULL pointer.
        /// 
        /// BFFC09F0  Instrument status error.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF     VISA     Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP   Driver Warnings
        ///           
        /// BFFF0000 to BFFFFFFF     VISA     Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP   Driver Errors
        /// 
        /// </returns>
        public int GetRegister(int Registers, int Channel, out int Value)
        {
            int pInvokeResult = PInvoke.GetRegister(this._handle, Registers, Channel, out Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function configures the remote mode.
        /// 
        /// Attribute(s):
        /// HMP4000_ATTR_SYSTEM_LOCAL
        /// HMP4000_ATTR_SYSTEM_REMOTE
        /// HMP4000_ATTR_SYSTEM_REMOTE_LOCK
        /// HMP4000_ATTR_SYSTEM_REMOTE_MIX
        /// 
        /// Remote-control command(s):
        /// SYSTem:LOCal
        /// SYSTem:REMote
        /// SYSTem:RWLock
        /// SYSTem:MIX
        /// </summary>
        /// <param name="Remote_Mode">
        /// This control selects the remote mode.
        /// 
        /// Valid Values:
        /// HMP4000_VAL_LOCAL       (0) - Local
        /// HMP4000_VAL_REMOTE      (1) - Remote
        /// HMP4000_VAL_REMOTE_LOCK (2) - Remote Lock
        /// HMP4000_VAL_REMOTE_MIX  (3) - Remote Mix
        /// 
        /// Default Value: HMP4000_VAL_LOCAL (0)
        /// 
        /// Note(s):
        /// 
        /// (1) Local: Sets the system to front panel control. The front panel control is unlocked.
        /// 
        /// (2) Remote: Sets the system to remote state. The front panel control is locked. By pushing the REMOTE button the front panel control will be activated. If the instrument receives a remote command it will be switched into remote control automatically (REMOTE button LED lights up).
        /// 
        /// (3) Remote Lock: Sets the system to remote state. The front panel control is locked and can not be unlocked via REMOTE button). You are only able to unlock the front panel control via function hmp4000_RemoteMode - Local.
        /// 
        /// (4) Remote Mix: Sets the system to remote state. The front panel and remote control are possible simultaneously (mixed mode).
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex) Status Code Types
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF VISA Warnings
        /// 3FFC0000 to 3FFCFFFF VXIPnP Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF VISA Errors
        /// BFFC0000 to BFFCFFFF VXIPnP Driver Errors
        /// </returns>
        public int RemoteMode(int Remote_Mode)
        {
            int pInvokeResult = PInvoke.RemoteMode(this._handle, Remote_Mode);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function clears status.
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// Remote-control command(s):
        /// *CLS
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex) Status Code Types
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF VISA Warnings
        /// 3FFC0000 to 3FFCFFFF VXIPnP Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF VISA Errors
        /// BFFC0000 to BFFCFFFF VXIPnP Driver Errors
        /// </returns>
        public int ClearStatus()
        {
            int pInvokeResult = PInvoke.ClearStatus(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function returns the ID Query response string.
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// Remote-control command(s):
        /// *IDN?
        /// </summary>
        /// <param name="ID_Query_Response">
        /// Returns the ID Query response string. The array should consist of at least 256 elements.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex) Status Code Types
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF VISA Warnings
        /// 3FFC0000 to 3FFCFFFF VXIPnP Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF VISA Errors
        /// BFFC0000 to BFFCFFFF VXIPnP Driver Errors
        /// </returns>
        public int IDQueryResponse(System.Text.StringBuilder ID_Query_Response)
        {
            int pInvokeResult = PInvoke.IDQueryResponse(this._handle, ID_Query_Response);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// Stops further command processing until all commands sent before *WAI have been executed.
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// Remote-control command(s):
        /// *WAI
        /// </summary>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex) Status Code Types
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF VISA Warnings
        /// 3FFC0000 to 3FFCFFFF VXIPnP Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF VISA Errors
        /// BFFC0000 to BFFCFFFF VXIPnP Driver Errors
        /// </returns>
        public int ProcessAllPreviousCommands()
        {
            int pInvokeResult = PInvoke.ProcessAllPreviousCommands(this._handle);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the OPC.
        /// 
        /// Attribute(s):
        /// -
        /// 
        /// Remote-control command(s):
        /// *OPC?
        /// </summary>
        /// <param name="OPC">
        /// Queries the OPC.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        ///           
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This instrument driver also returns errors and warnings defined by other sources. The following table defines the ranges of additional status codes that this driver can return. The table lists the different include files that contain the defined constants for the particular status codes:
        /// 
        /// Numeric Range (in Hex) Status Code Types
        /// -------------------------------------------------
        /// 3FFF0000 to 3FFFFFFF VISA Warnings
        /// 3FFC0000 to 3FFCFFFF VXIPnP Driver Warnings
        /// 
        /// BFFF0000 to BFFFFFFF VISA Errors
        /// BFFC0000 to BFFCFFFF VXIPnP Driver Errors
        /// </returns>
        public int QueryOPC(out int OPC)
        {
            int pInvokeResult = PInvoke.QueryOPC(this._handle, out OPC);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function writes a user-specified string to the instrument.
        /// 
        /// Note:
        /// 
        /// This function bypasses attribute state caching. Therefore, when you call this function, the cached values for all attributes will be invalidated.
        /// 
        /// </summary>
        /// <param name="Write_Buffer">
        /// Pass the string to be written to the instrument.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int WriteInstrData(string Write_Buffer)
        {
            int pInvokeResult = PInvoke.WriteInstrData(this._handle, Write_Buffer);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function reads data from the instrument.
        /// </summary>
        /// <param name="Number_of_Bytes_To_Read">
        /// Pass the maximum number of bytes to read from the instruments.  
        /// 
        /// Valid Range:  0 to the number of elements in the Read Buffer.
        /// 
        /// Default Value:  0
        /// 
        /// 
        /// </param>
        /// <param name="Read_Buffer">
        /// After this function executes, this parameter contains the data that was read from the instrument.
        /// </param>
        /// <param name="Num_Bytes_Read">
        /// Returns the number of bytes actually read from the instrument and stored in the Read Buffer.
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int ReadInstrData(int Number_of_Bytes_To_Read, System.Text.StringBuilder Read_Buffer, out uint Num_Bytes_Read)
        {
            int pInvokeResult = PInvoke.ReadInstrData(this._handle, Number_of_Bytes_To_Read, Read_Buffer, out Num_Bytes_Read);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the ViBoolean value.
        /// 
        /// 
        /// </summary>
        /// <param name="Command">
        /// The query to be sent to the instrument.
        /// 
        /// Default Value:  ""
        /// </param>
        /// <param name="Value">
        /// Returns the ViBoolean value
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int QueryViBoolean(string Command, out bool Value)
        {
            ushort ValueAsUShort;
            int pInvokeResult = PInvoke.QueryViBoolean(this._handle, Command, out ValueAsUShort);
            Value = System.Convert.ToBoolean(ValueAsUShort);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the ViInt32 value.
        /// 
        /// 
        /// </summary>
        /// <param name="Command">
        /// The query to be sent to the instrument.
        /// 
        /// Default Value:  ""
        /// </param>
        /// <param name="Value">
        /// Returns the ViInt32 value
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int QueryViInt32(string Command, out int Value)
        {
            int pInvokeResult = PInvoke.QueryViInt32(this._handle, Command, out Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the ViReal64 value.
        /// 
        /// 
        /// </summary>
        /// <param name="Command">
        /// The query to be sent to the instrument.
        /// 
        /// Default Value:  ""
        /// </param>
        /// <param name="Value">
        /// Returns the ViReal64 value
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int QueryViReal64(string Command, out double Value)
        {
            int pInvokeResult = PInvoke.QueryViReal64(this._handle, Command, out Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        /// <summary>
        /// This function queries the ViString value.
        /// 
        /// 
        /// </summary>
        /// <param name="Command">
        /// The query to be sent to the instrument.
        /// 
        /// Default Value:  ""
        /// </param>
        /// <param name="Buffer_Size">
        /// Pass the number of bytes in the ViChar array you specify for the  Value parameter.  
        /// 
        /// If the current value of the attribute, including the terminating NUL byte, contains more bytes that you indicate in this parameter, the function copies Buffer Size - 1 bytes into the buffer, places an ASCII NUL byte at the end of the buffer, and returns the buffer size you must pass to get the entire value.  For example, if the value is "123456" and the Buffer Size is 4, the function places "123" into the buffer and returns 7.
        /// 
        /// If you pass a negative number, the function copies the value to the buffer regardless of the number of bytes in the value.
        /// 
        /// If you pass 0, you can pass VI_NULL for the Value buffer parameter.
        /// 
        /// Default Value: 512
        /// </param>
        /// <param name="Value">
        /// Returns the ViBoolean value
        /// 
        /// </param>
        /// <returns>
        /// Returns the status code of this operation.  The status code  either indicates success or describes an error or warning condition.  You examine the status code from each call to an instrument driver function to determine if an error occurred.
        /// 
        /// To obtain a text description of the status code, call the hmp4000_error_message function.  To obtain additional information about the error condition, call the hmp4000_GetError function.  To clear the error information from the driver, call the hmp4000_ClearError function.
        /// 
        /// The general meaning of the status code is as follows:
        /// 
        /// Value                  Meaning
        /// -------------------------------
        /// 0                      Success
        /// Positive Values        Warnings
        /// Negative Values        Errors
        /// 
        /// This driver defines the following status codes:
        ///           
        /// Status    Description
        /// -------------------------------------------------
        /// WARNINGS:
        /// 3FFA2001  The instrument was in an uncalibrated state
        ///           when the measurement was taken.
        /// 3FFA2002  The measurement taken was over the instrument's
        ///           range.
        ///   
        /// ERRORS:
        /// BFFA2001  The Active Marker is not enabled.
        /// BFFA2002  The Active Marker is not a delta marker.
        /// BFFA2003  The maximum waiting time for this operation was
        ///           exceeded.
        ///           
        /// This instrument driver also returns errors and warnings defined by other sources.  The following table defines the ranges of additional status codes that this driver can return.  The table lists the different include files that contain the defined constants for the particular status codes:
        ///           
        /// Numeric Range (in Hex)   Status Code Types    
        /// -------------------------------------------------
        /// 3FFA0000 to 3FFA1FFF     IVI    Warnings
        /// 3FFF0000 to 3FFFFFFF     VISA   Warnings
        /// 3FFC0000 to 3FFCFFFF     VXIPnP Driver Warnings
        ///           
        /// BFFA0000 to BFFA1FFF     IVI    Errors
        /// BFFF0000 to BFFFFFFF     VISA   Errors
        /// BFFC0000 to BFFCFFFF     VXIPnP Driver Errors
        /// 
        /// </returns>
        public int QueryViString(string Command, int Buffer_Size, System.Text.StringBuilder Value)
        {
            int pInvokeResult = PInvoke.QueryViString(this._handle, Command, Buffer_Size, Value);
            PInvoke.TestForError(this._handle, pInvokeResult);
            return pInvokeResult;
        }

        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if ((this._disposed == false))
            {
                PInvoke.close(this._handle);
                this._handle = new System.Runtime.InteropServices.HandleRef(null, System.IntPtr.Zero);
            }
            this._disposed = true;
        }

        public int GetInt32(hmp4000Properties propertyId, string repeatedCapabilityOrChannel)
        {
            int val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), out val));
            return val;
        }

        public int GetInt32(hmp4000Properties propertyId)
        {
            return this.GetInt32(propertyId, "");
        }

        public double GetDouble(hmp4000Properties propertyId, string repeatedCapabilityOrChannel)
        {
            double val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), out val));
            return val;
        }

        public double GetDouble(hmp4000Properties propertyId)
        {
            return this.GetDouble(propertyId, "");
        }

        public string GetString(hmp4000Properties propertyId, string repeatedCapabilityOrChannel)
        {
            System.Text.StringBuilder newVal = new System.Text.StringBuilder(512);
            int size = PInvoke.GetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), 512, newVal);
            if ((size < 0))
            {
                PInvoke.ThrowError(this._handle, size);
            }
            else
            {
                if ((size > 0))
                {
                    newVal.Capacity = size;
                    PInvoke.TestForError(this._handle, PInvoke.GetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), size, newVal));
                }
            }
            return newVal.ToString();
        }

        public string GetString(hmp4000Properties propertyId)
        {
            return this.GetString(propertyId, "");
        }

        public bool GetBoolean(hmp4000Properties propertyId, string repeatedCapabilityOrChannel)
        {
            ushort val;
            PInvoke.TestForError(this._handle, PInvoke.GetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), out val));
            return System.Convert.ToBoolean(val);
        }

        public bool GetBoolean(hmp4000Properties propertyId)
        {
            return this.GetBoolean(propertyId, "");
        }

        public void SetInt32(hmp4000Properties propertyId, string repeatedCapabilityOrChannel, int val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViInt32(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), val));
        }

        public void SetInt32(hmp4000Properties propertyId, int val)
        {
            this.SetInt32(propertyId, "", val);
        }

        public void SetDouble(hmp4000Properties propertyId, string repeatedCapabilityOrChannel, double val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViReal64(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), val));
        }

        public void SetDouble(hmp4000Properties propertyId, double val)
        {
            this.SetDouble(propertyId, "", val);
        }

        public void SetString(hmp4000Properties propertyId, string repeatedCapabilityOrChannel, string val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViString(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), val));
        }

        public void SetString(hmp4000Properties propertyId, string val)
        {
            this.SetString(propertyId, "", val);
        }

        public void SetBoolean(hmp4000Properties propertyId, string repeatedCapabilityOrChannel, bool val)
        {
            PInvoke.TestForError(this._handle, PInvoke.SetAttributeViBoolean(this._handle, repeatedCapabilityOrChannel, ((uint)(propertyId)), System.Convert.ToUInt16(val)));
        }

        public void SetBoolean(hmp4000Properties propertyId, bool val)
        {
            this.SetBoolean(propertyId, "", val);
        }

        private class PInvoke
        {

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_init", CallingConvention = CallingConvention.StdCall)]
            public static extern int init(string Resource_Name, ushort ID_Query, ushort Reset_Device, out System.IntPtr Instrument_Handle);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_InitWithOptions", CallingConvention = CallingConvention.StdCall)]
            public static extern int InitWithOptions(string Resource_Name, ushort ID_Query, ushort Reset_Device, string Option_String, out System.IntPtr Instrument_Handle);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureChannel", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureChannel(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Channel);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetChannel", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetChannel(System.Runtime.InteropServices.HandleRef Instrument_Handle, out int Channel);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureCombinedVoltageAndCurrent", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureCombinedVoltageAndCurrent(System.Runtime.InteropServices.HandleRef Instrument_Handle, double Voltage, double Current);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureOutputState", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureOutputState(System.Runtime.InteropServices.HandleRef Instrument_Handle, ushort Output_State);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureGeneralOutputState", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureGeneralOutputState(System.Runtime.InteropServices.HandleRef Instrument_Handle, ushort Output_State);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureOutputStateChannelOnly", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureOutputStateChannelOnly(System.Runtime.InteropServices.HandleRef Instrument_Handle, ushort Output_State);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureSettingsLocation", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureSettingsLocation(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Memory_Operation, int Location);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryCombinedVoltageAndCurrent", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryCombinedVoltageAndCurrent(System.Runtime.InteropServices.HandleRef Instrument_Handle, out double Voltage_Value, out double Current);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureArbitraryGeneratorWaveformData", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureArbitraryGeneratorWaveformData(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Array_Size, double[] Voltage, double[] Current, double[] Time, int Repetition);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureArbitraryGeneratorStartChannel", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureArbitraryGeneratorStartChannel(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Channel);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureArbitraryGeneratorStopChannel", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureArbitraryGeneratorStopChannel(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Channel);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureArbitraryGeneratorDataTransferChannel", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureArbitraryGeneratorDataTransferChannel(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Channel);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureArbitraryGeneratorDataStorage", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureArbitraryGeneratorDataStorage(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Memory_Operation, int Memory_Index);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureCurrentValue", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureCurrentValue(System.Runtime.InteropServices.HandleRef Instrument_Handle, double Current);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureElectronicFuse", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureElectronicFuse(System.Runtime.InteropServices.HandleRef Instrument_Handle, ushort Fuse, int Link_to_Channel, int Unlink_from_Channel);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureCurrentStepValue", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureCurrentStepValue(System.Runtime.InteropServices.HandleRef Instrument_Handle, double Current_Step);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryElectronicFuseStatus", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryElectronicFuseStatus(System.Runtime.InteropServices.HandleRef Instrument_Handle, out ushort Fuse_Status);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureVoltageValue", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureVoltageValue(System.Runtime.InteropServices.HandleRef Instrument_Handle, double Voltage);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureOverVoltageProtection", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureOverVoltageProtection(System.Runtime.InteropServices.HandleRef Instrument_Handle, double OVP_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureVoltageStepValue", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureVoltageStepValue(System.Runtime.InteropServices.HandleRef Instrument_Handle, double Voltage_Step);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryOverVoltageProtectionStatus", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryOverVoltageProtectionStatus(System.Runtime.InteropServices.HandleRef Instrument_Handle, out ushort OVP_Status);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ClearOverVoltageProtection", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearOverVoltageProtection(System.Runtime.InteropServices.HandleRef Instrument_Handle);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ConfigureBeeper", CallingConvention = CallingConvention.StdCall)]
            public static extern int ConfigureBeeper(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Beeper);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ReadVoltageOutput", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadVoltageOutput(System.Runtime.InteropServices.HandleRef Instrument_Handle, out double Voltage_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ReadCurrentOutput", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadCurrentOutput(System.Runtime.InteropServices.HandleRef Instrument_Handle, out double Current_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_reset", CallingConvention = CallingConvention.StdCall)]
            public static extern int reset(System.Runtime.InteropServices.HandleRef Instrument_Handle);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_self_test", CallingConvention = CallingConvention.StdCall)]
            public static extern int self_test(System.Runtime.InteropServices.HandleRef Instrument_Handle, out short Self_Test_Result, System.Text.StringBuilder Self_Test_Message);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_revision_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int revision_query(System.Runtime.InteropServices.HandleRef Instrument_Handle, System.Text.StringBuilder Instrument_Driver_Revision, System.Text.StringBuilder Firmware_Revision);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_SystemVersion", CallingConvention = CallingConvention.StdCall)]
            public static extern int SystemVersion(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Length, System.Text.StringBuilder System_Version);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_error_query", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_query(System.Runtime.InteropServices.HandleRef Instrument_Handle, out int Error_Code, System.Text.StringBuilder Error_Message);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_SetRegister", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetRegister(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Registers, int Channel, int Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetRegister", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRegister(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Registers, int Channel, out int Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_RemoteMode", CallingConvention = CallingConvention.StdCall)]
            public static extern int RemoteMode(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Remote_Mode);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ClearStatus", CallingConvention = CallingConvention.StdCall)]
            public static extern int ClearStatus(System.Runtime.InteropServices.HandleRef Instrument_Handle);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_IDQueryResponse", CallingConvention = CallingConvention.StdCall)]
            public static extern int IDQueryResponse(System.Runtime.InteropServices.HandleRef Instrument_Handle, System.Text.StringBuilder ID_Query_Response);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ProcessAllPreviousCommands", CallingConvention = CallingConvention.StdCall)]
            public static extern int ProcessAllPreviousCommands(System.Runtime.InteropServices.HandleRef Instrument_Handle);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryOPC", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryOPC(System.Runtime.InteropServices.HandleRef Instrument_Handle, out int OPC);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_WriteInstrData", CallingConvention = CallingConvention.StdCall)]
            public static extern int WriteInstrData(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Write_Buffer);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_ReadInstrData", CallingConvention = CallingConvention.StdCall)]
            public static extern int ReadInstrData(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Number_of_Bytes_To_Read, System.Text.StringBuilder Read_Buffer, out uint Num_Bytes_Read);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryViBoolean(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Command, out ushort Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryViInt32(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Command, out int Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryViReal64(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Command, out double Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_QueryViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int QueryViString(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Command, int Buffer_Size, System.Text.StringBuilder Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_close", CallingConvention = CallingConvention.StdCall)]
            public static extern int close(System.Runtime.InteropServices.HandleRef Instrument_Handle);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_error_message", CallingConvention = CallingConvention.StdCall)]
            public static extern int error_message(System.Runtime.InteropServices.HandleRef Instrument_Handle, int Error_Code, System.Text.StringBuilder Error_Message_2);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetAttributeRepeatedCapabilityIds", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeRepeatedCapabilityIds(System.Runtime.InteropServices.HandleRef Instrument_Handle, uint Attribute_ID, int Buffer_Size, System.Text.StringBuilder Repeated_Capability_Id_s_);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetAttributeRepeatedCapabilityIdNames", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeRepeatedCapabilityIdNames(System.Runtime.InteropServices.HandleRef Instrument_Handle, uint Attribute_ID, string Repeated_Capability_Id, int Buffer_Size, System.Text.StringBuilder Repeated_Capability_Id_Name_s_);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViInt32(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, out int Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViReal64(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, out double Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViString(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, int Buffer_Size, System.Text.StringBuilder Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViBoolean(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, out ushort Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetAttributeViSession(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, out System.Runtime.InteropServices.HandleRef Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_SetAttributeViInt32", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViInt32(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, int Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_SetAttributeViReal64", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViReal64(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, double Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_SetAttributeViString", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViString(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, string Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_SetAttributeViBoolean", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViBoolean(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, ushort Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_SetAttributeViSession", CallingConvention = CallingConvention.StdCall)]
            public static extern int SetAttributeViSession(System.Runtime.InteropServices.HandleRef Instrument_Handle, string Repeated_Capability_Name, uint Attribute_ID, System.Runtime.InteropServices.HandleRef Attribute_Value);

            [DllImport("hmp4000_64.dll", EntryPoint = "hmp4000_GetError", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetError(System.Runtime.InteropServices.HandleRef Instrument_Handle, out int Code, int BufferSize, System.Text.StringBuilder Description);


            public static int TestForError(System.Runtime.InteropServices.HandleRef handle, int status)
            {
                if ((status < 0))
                {
                    PInvoke.ThrowError(handle, status);
                }
                return status;
            }

            public static int ThrowError(System.Runtime.InteropServices.HandleRef handle, int code)
            {
                int status;
                int size = PInvoke.GetError(handle, out status, 0, null);
                System.Text.StringBuilder msg = new System.Text.StringBuilder();
                if ((size >= 0))
                {
                    msg.Capacity = size;
                    PInvoke.GetError(handle, out status, size, msg);
                }
                throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
            }
        }
    }

    public class hmp4000Constants
    {

        public const int Output1 = 1;

        public const int Output2 = 2;

        public const int Output3 = 3;

        public const int Output4 = 4;

        public const int MemOperationStore = 0;

        public const int MemOperationRecall = 1;

        public const int Ch1 = 1;

        public const int Ch2 = 2;

        public const int Ch3 = 3;

        public const int Ch4 = 4;

        public const int Mem1 = 1;

        public const int Mem2 = 2;

        public const int Mem3 = 3;

        public const int MemOperationClear = 2;

        public const int ChN = 0;

        public const int BeeperOff = 0;

        public const int BeeperOn = 1;

        public const int BeeperCritical = 2;

        public const int QuestionableRegister = 0;

        public const int QuestionableInstrumentRegister = 1;

        public const int QuestionableInstrumentSpecificRegister = 2;

        public const int QuestionableInstrumentSpecificConditionRegister = 3;

        public const int Local = 0;

        public const int Remote = 1;

        public const int RemoteLock = 2;

        public const int RemoteMix = 3;
    }

    public enum hmp4000Properties
    {

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrRangeCheck = 1050002,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrQueryInstrumentStatus = 1050003,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrCache = 1050004,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrSimulate = 1050005,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrRecordCoercions = 1050006,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrInterchangeCheck = 1050021,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrSpy = 1050022,

        /// <summary>
        /// System.Boolean
        /// </summary>
        RsAttrUseSpecificSimulation = 1050023,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrClassDriverDescription = 1050518,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrClassDriverPrefix = 1050301,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrClassDriverVendor = 1050517,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrClassDriverRevision = 1050552,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrClassDriverClassSpecMajorVersion = 1050519,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrClassDriverClassSpecMinorVersion = 1050520,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrSpecificDriverDescription = 1050514,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrSpecificDriverPrefix = 1050302,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrSpecificDriverLocator = 1050303,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrSpecificDriverVendor = 1050513,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrSpecificDriverRevision = 1050551,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrSpecificDriverClassSpecMajorVersion = 1050515,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrSpecificDriverClassSpecMinorVersion = 1050516,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrSupportedInstrumentModels = 1050327,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrGroupCapabilities = 1050401,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrFunctionCapabilities = 1050402,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrChannelCount = 1050203,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrDriverSetup = 1050007,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrInstrumentManufacturer = 1050511,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrInstrumentModel = 1050512,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrInstrumentFirmwareRevision = 1050510,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrOptionsList = 1050591,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrIoResourceDescriptor = 1050304,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrLogicalName = 1050305,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrPrimaryError = 1050101,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrSecondaryError = 1050102,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrErrorElaboration = 1050103,

        /// <summary>
        /// System.IntPtr
        /// </summary>
        RsAttrVisaRmSession = 1050321,

        /// <summary>
        /// System.IntPtr
        /// </summary>
        RsAttrIoSession = 1050322,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrIoSessionType = 1050324,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrSpecificDriverMajorVersion = 1050503,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrSpecificDriverMinorVersion = 1050504,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrSpecificDriverMinorMinorVersion = 1050590,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrClassDriverMajorVersion = 1050505,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrClassDriverMinorVersion = 1050506,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrEngineMajorVersion = 1050501,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrEngineMinorVersion = 1050502,

        /// <summary>
        /// System.String
        /// </summary>
        RsAttrEngineRevision = 1050553,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrOpcCallback = 1050602,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrCheckStatusCallback = 1050603,

        /// <summary>
        /// System.Int32
        /// </summary>
        RsAttrOpcTimeout = 1051001,

        /// <summary>
        /// System.Int32
        /// </summary>
        SelectionOfChannel = 1150049,

        /// <summary>
        /// System.Boolean
        /// </summary>
        OutputState = 1150001,

        /// <summary>
        /// System.Boolean
        /// </summary>
        GeneralOutputState = 1150054,

        /// <summary>
        /// System.Boolean
        /// </summary>
        OutputSelect = 1150055,

        /// <summary>
        /// System.Int32
        /// </summary>
        SettingsStoreLocation = 1150002,

        /// <summary>
        /// System.Int32
        /// </summary>
        SettingsRecallLocation = 1150003,

        /// <summary>
        /// System.Int32
        /// </summary>
        BeeperMode = 1150004,

        /// <summary>
        /// System.Double
        /// </summary>
        OutputVoltageLevel = 1150005,

        /// <summary>
        /// System.Double
        /// </summary>
        OutputVoltageLevelStep = 1150006,

        /// <summary>
        /// System.Double
        /// </summary>
        OverVoltageProtectionLevel = 1150007,

        /// <summary>
        /// System.Boolean
        /// </summary>
        QueryOverVoltageProtectionStatus = 1150008,

        /// <summary>
        /// System.String
        /// </summary>
        OverVoltageProtectionClear = 1150009,

        /// <summary>
        /// System.Double
        /// </summary>
        OutputCurrentLevel = 1150010,

        /// <summary>
        /// System.Double
        /// </summary>
        OutputCurrentLevelStep = 1150011,

        /// <summary>
        /// System.Boolean
        /// </summary>
        ElectronicFuseState = 1150012,

        /// <summary>
        /// System.Boolean
        /// </summary>
        QueryElectronicFuseStatus = 1150013,

        /// <summary>
        /// System.Int32
        /// </summary>
        ElectronicFuseChannelLink = 1150014,

        /// <summary>
        /// System.Int32
        /// </summary>
        ElectronicFuseChannelUnlink = 1150015,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbitraryGeneratorRepetition = 1150016,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbitraryGeneratorStartChannel = 1150017,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbitraryGeneratorStopChannel = 1150018,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbitraryGeneratorDataTransferChannel = 1150019,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbitraryGeneratorSave = 1150020,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbitraryGeneratorRecall = 1150021,

        /// <summary>
        /// System.Int32
        /// </summary>
        ArbitraryGeneratorClear = 1150022,

        /// <summary>
        /// System.Double
        /// </summary>
        MeasurementDcVoltage = 1150023,

        /// <summary>
        /// System.Double
        /// </summary>
        MeasurementDcCurrent = 1150024,

        /// <summary>
        /// System.String
        /// </summary>
        SystemVersion = 1150025,

        /// <summary>
        /// System.String
        /// </summary>
        SystemError = 1150026,

        /// <summary>
        /// System.String
        /// </summary>
        SystemAllErrors = 1150027,

        /// <summary>
        /// System.String
        /// </summary>
        SystemLocal = 1150050,

        /// <summary>
        /// System.String
        /// </summary>
        SystemRemote = 1150051,

        /// <summary>
        /// System.String
        /// </summary>
        SystemRemoteLock = 1150052,

        /// <summary>
        /// System.String
        /// </summary>
        SystemRemoteMix = 1150053,

        /// <summary>
        /// System.Int32
        /// </summary>
        QuestionableEnableRegister = 1150029,

        /// <summary>
        /// System.Int32
        /// </summary>
        QuestionableInstrumentEnableRegister = 1150030,

        /// <summary>
        /// System.Int32
        /// </summary>
        QuestionableInstrumentSpecificEnableRegister = 1150031,

        /// <summary>
        /// System.Int32
        /// </summary>
        QuestionableInstrumentSpecificConditionRegister = 1150032,

        /// <summary>
        /// System.String
        /// </summary>
        IdQueryResponse = 1150033,
    }
}
