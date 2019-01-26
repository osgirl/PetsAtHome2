﻿// LibPdInstance.cs - Unity integration of libpd, supporting multiple instances.
// -----------------------------------------------------------------------------
// Copyright (c) 2018 Niall Moody
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#region UnityEvent types
//------------------------------------------------------------------------------
/// Single string parameter event type (used for Bang events).
[System.Serializable]
public class StringEvent : UnityEvent<string> {}

/// String + float parameter event type (used for Float events).
[System.Serializable]
public class StringFloatEvent : UnityEvent<string, float> {}

/// String + string parameter event type (used for Symbol events).
[System.Serializable]
public class StringStringEvent : UnityEvent<string, string> {}

/// String + object array parameter event type (used for List events).
[System.Serializable]
public class StringObjArrEvent : UnityEvent<string, object[]> {}

/// String + object array parameter event type (used for Message events).
[System.Serializable]
public class StringStringObjArrEvent : UnityEvent<string, string, object[]> {}

/// Int + int + int parameter event type (used for various MIDI events).
[System.Serializable]
public class IntIntIntEvent : UnityEvent<int, int, int> {}

/// Int + int parameter event type (used for various MIDI events).
[System.Serializable]
public class IntIntEvent : UnityEvent<int, int> {}
#endregion

/// <summary>
/// Unity Component for running a Pure Data patch. Uses libpd's new multiple
/// instance support, so you can run multiple LibPdInstances in your scene.
/// 
/// <para>
/// Pd patches should be stored in Assets/StreamingAssets/PdAssets and assigned
/// to LibPdInstance in the inspector (type the patch name (minus .pd) into the
/// Patch text box).
/// </para>
/// </summary>
/// 
/// <remarks>
/// This uses the basic c version of libpd over the C# bindings, as I was unable
/// to get the C# bindings working with Unity. This is likely due to my own
/// inexperience with C# (I'm primarily a C++ programmer), rather than an issue
/// with the lipbd C# bindings themselves.
/// 
/// Along those lines, I modelled parts of this class after the C# bindings, so
/// you will likely see some duplicated code.
/// 
/// Also, as it stands, this requires a small patch to z_libpd.c to allow us to
/// install our own print hook (so we can pipe print messages to Unity's
/// Console). Unfortunately, libpd requires the print hook to be set up before
/// libpd_init() is called, and will not accept any changes after that.
/// 
/// This causes major problems with Unity, as we want to set the print hook when
/// we start our game, and clear it when the game exits. However, because Unity
/// keeps native dlls active as long as the editor is running, libpd_init()
/// (being a one-time function) will effectively never get called again, and the
/// print hook will remain set to the value set when we first ran our game from
/// the editor. The result: if we try to run our game from the editor a second
/// time, we crash the entire editor.
/// 
/// For this reason the repository for this code includes pre-built libpd
/// binaries which include the print hook patch.
/// 
/// If you're building libpd from source yourself, you can get around this issue
/// by adding the following line to the end of libpd_set_printhook() in
/// z_libpd.c:
/// 
/// sys_printhook = libpd_printhook;
/// 
/// 
/// Note: LibPdInstance is all implemented as a single file because I find
/// single file libraries easier to integrate into my own projects. This may
/// change in future.
/// 
/// </remarks>
[RequireComponent(typeof(AudioSource))]
public class LibPdInstance : MonoBehaviour {

	#region libpd imports
	//--------------------------------------------------------------------------
	/// libpd functions that we need to be able to call from C#.
	[DllImport("libpd")]
	private static extern int libpd_init();

	[DllImport("libpd")]
	private static extern void libpd_clear_search_path();

	[DllImport("libpd")]
	private static extern void libpd_add_to_search_path([In] [MarshalAs(UnmanagedType.LPStr)] string s);

	[DllImport("libpd")]
	private static extern IntPtr libpd_new_instance();

	[DllImport("libpd")]
	private static extern void libpd_set_instance(IntPtr instance);

	[DllImport("libpd")]
	private static extern void libpd_free_instance(IntPtr instance);

	[DllImport("libpd")]
	private static extern int libpd_init_audio(int inChans, int outChans, int sampleRate);

	[DllImport("libpd")]
	private static extern IntPtr libpd_openfile([In] [MarshalAs(UnmanagedType.LPStr)] string basename,
												[In] [MarshalAs(UnmanagedType.LPStr)] string dirname);

	[DllImport("libpd")]
	private static extern void libpd_closefile(IntPtr p);

	[DllImport("libpd")]
	private static extern int libpd_process_float(int ticks,
												  [In] float[] inBuffer,
												  [Out] float[] outBuffer);

	[DllImport("libpd")]
	private static extern int libpd_blocksize();

	[DllImport("libpd")]
	private static extern int libpd_start_message(int max_length);

	[DllImport("libpd")]
	private static extern void libpd_add_float(float x);

	[DllImport("libpd")]
	private static extern void libpd_add_symbol([In] [MarshalAs(UnmanagedType.LPStr)] string sym);

	[DllImport("libpd")]
	private static extern int libpd_finish_list([In] [MarshalAs(UnmanagedType.LPStr)] string recv);

	[DllImport("libpd")]
	private static extern int libpd_finish_message([In] [MarshalAs(UnmanagedType.LPStr)] string recv,
												   [In] [MarshalAs(UnmanagedType.LPStr)] string msg);

	[DllImport("libpd")]
	private static extern int libpd_bang([In] [MarshalAs(UnmanagedType.LPStr)] string recv);

	[DllImport("libpd")]
	private static extern int libpd_float([In] [MarshalAs(UnmanagedType.LPStr)] string recv,
										  float x);

	[DllImport("libpd")]
	private static extern int libpd_symbol([In] [MarshalAs(UnmanagedType.LPStr)] string recv,
										   [In] [MarshalAs(UnmanagedType.LPStr)] string sym);

	[DllImport("libpd")]
	private static extern int libpd_exists([In] [MarshalAs(UnmanagedType.LPStr)] string obj);

	[DllImport("libpd")]
	private static extern IntPtr libpd_bind([In] [MarshalAs(UnmanagedType.LPStr)] string symbol);

	[DllImport("libpd")]
	private static extern void libpd_unbind(IntPtr binding);

	[DllImport("libpd")]
	private static extern void libpd_set_verbose(int verbose);

	[DllImport("libpd")]
	private static extern int libpd_is_float(IntPtr atom);

	[DllImport("libpd")]
	private static extern int libpd_is_symbol(IntPtr atom);

	[DllImport("libpd")]
	private static extern float libpd_get_float(IntPtr atom);

	[DllImport("libpd")]
	private static extern IntPtr libpd_get_symbol(IntPtr atom);

	[DllImport("libpd")]
	private static extern IntPtr libpd_next_atom(IntPtr atom);

	[DllImport("libpd")]
	private static extern int libpd_noteon(int channel,
										   int pitch,
										   int velocity);

	[DllImport("libpd")]
	private static extern int libpd_controlchange(int channel,
												  int controller,
												  int value);

	[DllImport("libpd")]
	private static extern int libpd_programchange(int channel, int program);

	[DllImport("libpd")]
	private static extern int libpd_pitchbend(int channel, int value);

	[DllImport("libpd")]
	private static extern int libpd_aftertouch(int channel, int value);

	[DllImport("libpd")]
	private static extern int libpd_polyaftertouch(int channel,
												   int pitch,
												   int value);

	[DllImport("libpd")]
	private static extern int libpd_midibyte(int port, int value);

	[DllImport("libpd")]
	private static extern int libpd_sysex(int port, int value);

	[DllImport("libpd")]
	private static extern int libpd_sysrealtime(int port, int value);

	[DllImport("libpd")]
	private static extern int libpd_arraysize([In] [MarshalAs(UnmanagedType.LPStr)] string name);

	[DllImport("libpd")]
	private static extern int libpd_read_array([Out] float[] dest,
											   [In] [MarshalAs(UnmanagedType.LPStr)] string src,
											   int offset,
											   int n);

	[DllImport("libpd")]
	private static extern int libpd_write_array([In] [MarshalAs(UnmanagedType.LPStr)] string dest,
												int offset,
												[In] float[] src,
												int n);
	#endregion

	#region libpd delegate/callback declarations
	//--------------------------------------------------------------------------
	//-Print hook---------------------------------------------------------------
	// We don't make the print hook publicly available (for now), so it's just a
	//single static delegate.

	/// Delegate/function pointer type.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdPrintHook([In] [MarshalAs(UnmanagedType.LPStr)] string message);

	/// libpd function for setting the hook.
	[DllImport("libpd")]
	private static extern void libpd_set_printhook(LibPdPrintHook hook);

	/// Instance of the print hook, kept to ensure it doesn't get garbage
	/// collected.
	private static LibPdPrintHook printHook;

	//-Bang hook----------------------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdBangHook([In] [MarshalAs(UnmanagedType.LPStr)] string symbol);
	
	[DllImport("libpd")]
	private static extern void libpd_set_banghook(LibPdBangHook hook);
	
	private LibPdBangHook bangHook;

	//-Float hook---------------------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdFloatHook([In] [MarshalAs(UnmanagedType.LPStr)] string symbol,
										float val);
	
	[DllImport("libpd")]
	private static extern void libpd_set_floathook(LibPdFloatHook hook);
	
	private LibPdFloatHook floatHook;

	//-Symbol hook--------------------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdSymbolHook([In] [MarshalAs(UnmanagedType.LPStr)] string symbol,
										 [In] [MarshalAs(UnmanagedType.LPStr)] string val);
	
	[DllImport("libpd")]
	private static extern void libpd_set_symbolhook(LibPdSymbolHook hook);
	
	private LibPdSymbolHook symbolHook;

	//-List hook----------------------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdListHook([In] [MarshalAs(UnmanagedType.LPStr)] string source,
									   int argc,
									   IntPtr argv);
	
	[DllImport("libpd")]
	private static extern void libpd_set_listhook(LibPdListHook hook);
	
	private LibPdListHook listHook;

	//-Message hook-------------------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMessageHook([In] [MarshalAs(UnmanagedType.LPStr)] string source,
										  [In] [MarshalAs(UnmanagedType.LPStr)] string symbol,
										  int argc,
										  IntPtr argv);
	
	[DllImport("libpd")]
	private static extern void libpd_set_messagehook(LibPdMessageHook hook);
	
	private LibPdMessageHook messageHook;

	//-MIDI Note On hook--------------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMidiNoteOnHook(int channel,
											 int pitch,
											 int velocity);

	[DllImport("libpd")]
	private static extern void libpd_set_noteonhook(LibPdMidiNoteOnHook hook);

	private LibPdMidiNoteOnHook noteOnHook;

	//-MIDI Control Change hook-------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMidiControlChangeHook(int channel,
													int controller,
													int value);

	[DllImport("libpd")]
	private static extern void libpd_set_controlchangehook(LibPdMidiControlChangeHook hook);

	private LibPdMidiControlChangeHook controlChangeHook;

	//-MIDI Program Change hook-------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMidiProgramChangeHook(int channel, int program);

	[DllImport("libpd")]
	private static extern void libpd_set_programchangehook(LibPdMidiProgramChangeHook hook);

	private LibPdMidiProgramChangeHook programChangeHook;

	//-MIDI Pitch Bend hook-----------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMidiPitchBendHook(int channel, int value);

	[DllImport("libpd")]
	private static extern void libpd_set_pitchbendhook(LibPdMidiPitchBendHook hook);

	private LibPdMidiPitchBendHook pitchBendHook;

	//-MIDI Aftertouch hook-----------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMidiAftertouchHook(int channel, int value);

	[DllImport("libpd")]
	private static extern void libpd_set_aftertouchhook(LibPdMidiAftertouchHook hook);

	private LibPdMidiAftertouchHook aftertouchHook;

	//-MIDI Polyphonic Aftertouch hook------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMidiPolyAftertouchHook(int channel, int pitch, int value);

	[DllImport("libpd")]
	private static extern void libpd_set_polyaftertouchhook(LibPdMidiPolyAftertouchHook hook);

	private LibPdMidiPolyAftertouchHook polyAftertouchHook;

	//-MIDI Byte hook-----------------------------------------------------------
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LibPdMidiByteHook(int channel, int value);

	[DllImport("libpd")]
	private static extern void libpd_set_midibytehook(LibPdMidiByteHook hook);

	private LibPdMidiByteHook midiByteHook;
	#endregion

	#region member variables
	//--------------------------------------------------------------------------
	/// The Pd patch this instance is running.
	[HideInInspector]
	public string patchName;
	///	The path to the directory of the Pd patch this instance is running.
	[HideInInspector]
	public string patchDir;

	#if UNITY_EDITOR
	/// This is a slightly tricky workaround we use so that we can drag and drop
	/// PD patches into the Inspector. By default, Unity doesn't let you do that
	/// with StreamingAssets, so we use the DefaultAsset type to get around that
	/// limitation. It's not perfect, but it's nicer than the alternative
	/// (typing the patch name by hand into a string box).
	/// 
	/// See also OnValidate().
	///	
	///	(We have to store Pd patches in StreamingAssets because libpd requires
	///	 us to load patches from files on the filesystem)
	public UnityEditor.DefaultAsset patch;
	#endif

	/// Hacky way of making pipePrintToConsoleStatic visible in the inspector.
	public bool pipePrintToConsole = false;
	/// Set to true to pipe any Pure Data *print* messages to Unity's console.
	public static bool pipePrintToConsoleStatic = false;

	/// Our pointer to the Pd patch this instance is running.
	IntPtr patchPointer;

	/// The Pd instance we're using.
	private IntPtr instance;
	/// The number of ticks to process at a time.
	private int numTicks;

	/// Any bindings we have for this patch.
	private Dictionary<string, IntPtr> bindings;

	/// True if we were unable to intialise Pd's audio processing.
	private bool pdFail = false;
	/// True if we were unable to open our patch.
	private bool patchFail = false;

	/// Global variable used to ensure we don't initialise LibPd more than once.
	private static bool pdInitialised = false;

	/// Holds any actions (events) sent from the audio thread.
	private List<System.Action> audioThreadActions = new List<System.Action>();
	/// Holds any actions (events) that have been copied over into the main thread to execute.
	private List<System.Action> mainThreadActions = new List<System.Action>();
	/// True if there are audioThreadActions pending.
	private volatile bool actionsPending = false;
	#endregion

	#region events
	//--------------------------------------------------------------------------
	/// Events placed in a struct so they don't clutter up the Inspector by default.
	[System.Serializable]
	public struct PureDataEvents {
		/// UnityEvent that will be invoked whenever we recieve a bang from the PD patch.
		public StringEvent Bang;
		/// UnityEvent that will be invoked whenever we recieve a float from the PD patch.
		public StringFloatEvent Float;
		/// UnityEvent that will be invoked whenever we recieve a symbol from the PD patch.
		public StringStringEvent Symbol;
		/// UnityEvent that will be invoked whenever we recieve a list from the PD patch.
		public StringObjArrEvent List;
		/// UnityEvent that will be invoked whenever we recieve a message from the PD patch.
		public StringStringObjArrEvent Message;
	};
	[Header("libpd -> Unity Events")]
	public PureDataEvents pureDataEvents;
	
	/// Events placed in a struct so they don't clutter up the Inspector by default.
	[System.Serializable]
	public struct MidiEvents {
		/// UnityEvent that will be invoked whenever we recieve a MIDI note on from the PD patch.
		public IntIntIntEvent MidiNoteOn;
		/// UnityEvent that will be invoked whenever we recieve a MIDI CC from the PD patch.
		public IntIntIntEvent MidiControlChange;
		/// UnityEvent that will be invoked whenever we recieve a MIDI program change from the PD patch.
		public IntIntEvent MidiProgramChange;
		/// UnityEvent that will be invoked whenever we recieve a MIDI pitch bend from the PD patch.
		public IntIntEvent MidiPitchBend;
		/// UnityEvent that will be invoked whenever we recieve a MIDI aftertouch from the PD patch.
		public IntIntEvent MidiAftertouch;
		/// UnityEvent that will be invoked whenever we recieve a polyphonic MIDI aftertouch from the PD patch.
		public IntIntIntEvent MidiPolyAftertouch;
		/// UnityEvent that will be invoked whenever we recieve a MIDI byte from the PD patch.
		public IntIntEvent MidiByte;
	};
	public MidiEvents midiEvents;
	#endregion
	
	#region MonoBehaviour methods
	//--------------------------------------------------------------------------
	/// Initialise LibPd.
	void Awake ()
	{
		// Initialise libpd, if it's not already.
		if(!pdInitialised)
		{
			// Setup hooks.
			printHook = new LibPdPrintHook(PrintOutput);
			libpd_set_printhook(printHook);
			
			bangHook = new LibPdBangHook(BangOutput);
			libpd_set_banghook(bangHook);
			
			floatHook = new LibPdFloatHook(FloatOutput);
			libpd_set_floathook(floatHook);
			
			symbolHook = new LibPdSymbolHook(SymbolOutput);
			libpd_set_symbolhook(symbolHook);
			
			listHook = new LibPdListHook(ListOutput);
			libpd_set_listhook(listHook);
			
			messageHook = new LibPdMessageHook(MessageOutput);
			libpd_set_messagehook(messageHook);
			
			noteOnHook = new LibPdMidiNoteOnHook(MidiNoteOnOutput);
			libpd_set_noteonhook(noteOnHook);

			controlChangeHook = new LibPdMidiControlChangeHook(MidiControlChangeOutput);
			libpd_set_controlchangehook(controlChangeHook);

			programChangeHook = new LibPdMidiProgramChangeHook(MidiProgramChangeOutput);
			libpd_set_programchangehook(programChangeHook);

			pitchBendHook = new LibPdMidiPitchBendHook(MidiPitchBendOutput);
			libpd_set_pitchbendhook(pitchBendHook);

			aftertouchHook = new LibPdMidiAftertouchHook(MidiAftertouchOutput);
			libpd_set_aftertouchhook(aftertouchHook);

			polyAftertouchHook = new LibPdMidiPolyAftertouchHook(MidiPolyAftertouchOutput);
			libpd_set_polyaftertouchhook(polyAftertouchHook);

			midiByteHook = new LibPdMidiByteHook(MidiByteOutput);
			libpd_set_midibytehook(midiByteHook);

			// Initialise libpd if possible, report any errors.
			int initErr = libpd_init();
			if(initErr != 0)
			{
				Debug.LogWarning("Warning; libpd_init() returned " + initErr);
				Debug.LogWarning("(if you're running this in the editor that probably just means this isn't the first time you've run your game, and is not a problem)");
			}
			pdInitialised = true;

			// Try and add the patch directory to libpd's search path for
			// loading externals (still can't seem to load externals when
			// running in Unity though).
			if(patchDir != String.Empty)
				libpd_add_to_search_path(Application.dataPath + patchDir);

			// Make sure our static pipePrintToConsole variable is set
			// correctly.
			pipePrintToConsoleStatic = pipePrintToConsole;
		}
		else
			pipePrintToConsole = pipePrintToConsoleStatic;

		if(pureDataEvents.Bang == null)
			pureDataEvents.Bang = new StringEvent();
		if(pureDataEvents.Float == null)
			pureDataEvents.Float = new StringFloatEvent();
		if(pureDataEvents.Symbol == null)
			pureDataEvents.Symbol = new StringStringEvent();
		if(pureDataEvents.List == null)
			pureDataEvents.List = new StringObjArrEvent();
		if(pureDataEvents.Message == null)
			pureDataEvents.Message = new StringStringObjArrEvent();

		if(midiEvents.MidiNoteOn == null)
			midiEvents.MidiNoteOn = new IntIntIntEvent();
		if(midiEvents.MidiControlChange == null)
			midiEvents.MidiControlChange = new IntIntIntEvent();
		if(midiEvents.MidiProgramChange == null)
			midiEvents.MidiProgramChange = new IntIntEvent();
		if(midiEvents.MidiPitchBend == null)
			midiEvents.MidiPitchBend = new IntIntEvent();
		if(midiEvents.MidiAftertouch == null)
			midiEvents.MidiAftertouch = new IntIntEvent();
		if(midiEvents.MidiPolyAftertouch == null)
			midiEvents.MidiPolyAftertouch = new IntIntIntEvent();
		if(midiEvents.MidiByte == null)
			midiEvents.MidiByte = new IntIntEvent();

		// Calc numTicks.
		int bufferSize;
		int noOfBuffers;

		AudioSettings.GetDSPBufferSize (out bufferSize, out noOfBuffers);
		numTicks = bufferSize/libpd_blocksize();

		// Create our instance.
		instance = libpd_new_instance();

		// Set our instance.
		libpd_set_instance(instance);

		// Initialise audio.
		int err = libpd_init_audio(2, 2, AudioSettings.outputSampleRate);
		if (err != 0)
		{
			pdFail = true;
			Debug.LogError(gameObject.name + ": Could not initialise Pure Data audio. Error = " + err);
		}
		else
		{
			if(patchName == String.Empty)
			{
				Debug.LogError(gameObject.name + ": No patch was assigned to this LibPdInstance.");
				patchFail = true;
			}
			else
			{
				//Create our bindings dictionary.
				bindings = new Dictionary<string, IntPtr>();

				// Open our patch.
				patchPointer = libpd_openfile(patchName + ".pd", Application.dataPath + patchDir);
				if(patchPointer == IntPtr.Zero)
				{
					Debug.LogError(gameObject.name + ": Could not open patch. Directory: " + (Application.dataPath + patchDir) + " Patch: " + patchName + ".pd");
					patchFail = true;
				}

				// Turn on audio processing.
				libpd_start_message(1);
				libpd_add_float(1.0f);
				libpd_finish_message("pd", "dsp");
			}
		}
	}
	
	//--------------------------------------------------------------------------
	/// Close the patch file on quit.
	void OnApplicationQuit ()
	{
		if(!pdFail && !patchFail)
		{
			libpd_set_instance(instance);

			libpd_start_message(1);
			libpd_add_float(0.0f);
			libpd_finish_message("pd", "dsp");

			//TODO: Is this correct? What happens if one LibPdInstance is
			//destroyed while another stays alive?
			if(printHook != null)
			{
				printHook = null;
				libpd_set_printhook(printHook);
			}

			foreach(var ptr in bindings.Values)
				libpd_unbind(ptr);
			bindings.Clear();

			libpd_closefile(patchPointer);
		}
	}
	
	//--------------------------------------------------------------------------
	/// We use this to dispatch any events that we've been sent from libpd.
	/// Any send/MIDI events sent from libpd will be sent from the audio thread,
	/// so we have to queue them and send them from the main thread, or Unity
	/// will get very upset.
	public void Update() {
		if(actionsPending) {
			mainThreadActions.Clear();

			//Copy events queued from audio thread.
			lock(audioThreadActions) {
				mainThreadActions.AddRange(audioThreadActions);

				audioThreadActions.Clear();
				actionsPending = false;
			}

			//Dispatch events on main thread.
			for(int i=0;i<mainThreadActions.Count;++i)
				mainThreadActions[i].Invoke();
		}
	}
	
	//--------------------------------------------------------------------------
	/// This function updates our static pipePrintToConsole variable when the
	/// public one changes, and ensures all other active LibPdInstances are
	/// updated too.
	private void OnValidate()
	{
		if(pipePrintToConsoleStatic != pipePrintToConsole)
		{
			pipePrintToConsoleStatic = pipePrintToConsole;

			LibPdInstance[] activePatches = FindObjectsOfType<LibPdInstance>();

			for(int i=0;i<activePatches.Length;++i)
				activePatches[i].pipePrintToConsole = pipePrintToConsoleStatic;
		}

		#if UNITY_EDITOR
		string lastName = patchName;

		//We use this to store the name of our PD patch as a string, as we need
		//to feed the filename and directory into libpd.
		if(patch != null)
			patchName = patch.name;

		if(lastName != patchName) {
			patchDir = AssetDatabase.GetAssetPath(patch.GetInstanceID());

			//Strip out "Assets", as the files won't be in the Assets folder in
			//a built version, only when running in the editor.
			patchDir = patchDir.Substring(patchDir.IndexOf("Assets") + 6);

			//Remove the name of the patch, as we only need the directory.
			patchDir = patchDir.Substring(0, patchDir.LastIndexOf('/') + 1);
		}
		#endif
	}

	//--------------------------------------------------------------------------
	/// Process audio.
	void OnAudioFilterRead (float[] data, int channels)
	{
		if(!pdFail && !patchFail)
		{
			libpd_set_instance(instance);
			libpd_process_float(numTicks, data, data);
		}
	}
	#endregion

	#region public methods
	//--------------------------------------------------------------------------
	/// Bind to a named object in the patch.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Bind(string symbol)
	{
		libpd_set_instance(instance);
		IntPtr ptr = libpd_bind(symbol);
		bindings.Add(symbol, ptr);
	}

	//--------------------------------------------------------------------------
	/// Release an existing binding.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void UnBind(string symbol)
	{
		libpd_set_instance(instance);
		libpd_unbind(bindings[symbol]);
		bindings.Remove(symbol);
	}

	//--------------------------------------------------------------------------
	/// Send a bang to the named receive object.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendBang(string receiver)
	{
		libpd_set_instance(instance);
	
		int err = libpd_bang(receiver);
	
		if(err == -1)
			Debug.LogWarning(gameObject.name + "::SendBang(): Could not find " + receiver + " object.");
	}



	//--------------------------------------------------------------------------
	/// Send a float to the named receive object.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendFloat(string receiver, float val)
	{
		libpd_set_instance(instance);
	
		int err = libpd_float(receiver, val);
	
		if(err == -1)
			Debug.LogWarning(gameObject.name + "::SendFloat(): Could not find " + receiver + " object.");
	}

	//--------------------------------------------------------------------------
	/// Send a symbol to the named receive object.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendSymbol(string receiver, string symbol)
	{
		libpd_set_instance(instance);
	
		if(libpd_symbol(receiver, symbol) != 0)
			Debug.LogWarning(gameObject.name + "::SendSymbol(): Could not find " + receiver + " object.");
	}

	//--------------------------------------------------------------------------
	///	Send a list to the named receive object.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendList(string receiver, params object[] args)
	{
		ProcessArgs(args);

		if(libpd_finish_list(receiver) != 0)
			Debug.LogWarning(gameObject.name + "::SendList(): Could not send list. receiver = " + receiver);
	}

	//--------------------------------------------------------------------------
	///	Send a message to the named receive object.
	///	<param name="destination">The name of the object to send the message
	///	 to.</param>
	///	<param name="symbol">The message keyword.</param>
	///	<param name="args">A list of values to send to the named object.</param>
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMessage(string destination,
							string symbol,
							params object[] args)
	{
		ProcessArgs(args);

		if(libpd_finish_message(destination, symbol) != 0)
			Debug.LogWarning(gameObject.name + "::SendMessage(): Could not send message. destination = " + destination + " symbol = " + symbol);
	}

	//--------------------------------------------------------------------------
	/// Send a MIDI note to the open patch.
	/// <param name="channel">The MIDI channel number (libpd expects channels to
	///  be in the range 0-15).</param>
	/// <param name="pitch">The MIDI note number (0-127).</param>
	/// <param name="velocity">The velocity of the note (0-127). Sending a
	///  velocity of 0 will usually be interpreted as a note off.</param>
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiNoteOn(int channel, int pitch, int velocity)
	{
		libpd_set_instance(instance);
		
		if(libpd_noteon(channel, pitch, velocity) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidiNoteOn(): input parameter(s) out of range. channel = " + channel + " pitch = " + pitch + " velocity = " + velocity);
	}

	//--------------------------------------------------------------------------
	/// Send a MIDI control change to the open patch.
	/// <param name="controller">The controller number (0-127).</param>
	/// <param name="value">The controller value (0-127).</param>
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiCc(int channel, int controller, int value)
	{
		libpd_set_instance(instance);

		if(libpd_controlchange(channel, controller, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidiCc(): input parameter(s) out of range. channel = " + channel + " controller = " + controller + " value = " + value);
	}

	//--------------------------------------------------------------------------
	/// Send a MIDI program change to the open patch.
	/// <param name="value">The program to change to (0-127).</param>
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiProgramChange(int channel, int value)
	{
		libpd_set_instance(instance);

		if(libpd_programchange(channel, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidiProgramChange(): input parameter(s) out of range. channel = " + channel + " value = " + value);
	}

	//--------------------------------------------------------------------------
	/// Send a MIDI pitch bend to the open patch.
	/// <param name ="value">The bend value has a  range -8192 -> +8192.</param>
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiPitchBend(int channel, int value)
	{
		libpd_set_instance(instance);

		if(libpd_pitchbend(channel, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidPitchBend(): input parameter(s) out of range. channel = " + channel + " value = " + value);
	}

	//--------------------------------------------------------------------------
	///	Send a MIDI aftertouch message to the open patch.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiAftertouch(int channel, int value)
	{
		libpd_set_instance(instance);

		if(libpd_aftertouch(channel, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidiAftertouch(): input parameter(s) out of range. channel = " + channel + " value = " + value);
	}

	//--------------------------------------------------------------------------
	///	Send a MIDI polyphonic aftertouch to the open patch.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiPolyAftertouch(int channel, int pitch, int value)
	{
		libpd_set_instance(instance);

		if(libpd_polyaftertouch(channel, pitch, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidiPolyAftertouch(): input parameter(s) out of range. channel = " + channel + " pitch = " + pitch + " value = " + value);
	}

	//--------------------------------------------------------------------------
	///	Send a MIDI byte(?) to the open patch.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiByte(int port, int value)
	{
		libpd_set_instance(instance);

		if(libpd_midibyte(port, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidiByte(): input parameter(s) out of range. port = " + port + " value = " + value);
	}

	//--------------------------------------------------------------------------
	///	Send a MIDI sysex byte(?) to the open patch.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiSysex(int port, int value)
	{
		libpd_set_instance(instance);

		if(libpd_sysex(port, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidSysex(): input parameter(s) out of range. port = " + port + " value = " + value);
	}

	//--------------------------------------------------------------------------
	///	Send a MIDI sysrealtime byte(?) to the open patch.
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void SendMidiSysRealtime(int port, int value)
	{
		libpd_set_instance(instance);

		if (libpd_sysrealtime(port, value) != 0)
			Debug.LogWarning(gameObject.name + "::SendMidiSysRealtime(): input parameter(s) out of range. port = " + port + " value = " + value);
	}

	//--------------------------------------------------------------------------
	///	Returns the size of the named array in the open patch.
	///	<returns>Returns a negative value if the named array doesn't
	///	exist.</returns>
	public int ArraySize(string name)
	{
		libpd_set_instance(instance);

		return libpd_arraysize(name);
	}

	//--------------------------------------------------------------------------
	///	Reads a set of values from the named array.
	///	<param name="dest">C# array to write the values of the Pd array into.
	///	 NOTE: make sure you allocate the array before you call this method, and
	///	 make sure its size is >= (count - offset).</param>
	///	<param name="src">Name of the array in your Pd patch.</param>
	///	<param name="offset">Offset into the Pd array to start reading
	///	 from.</param>
	///	<param name="count">Number of elements to read from the Pd array.</param>
	public void ReadArray(float[] dest, string src, int offset, int count)
	{
		libpd_set_instance(instance);

		//Note: the wiki says libpd_read_array() is supposed to return an error
		//code if the array doesn't exist or [offset -> offset+n] lies outside
		//the array, but looking at the code in z_libpd.c it will always return
		//0 (success). In case that ever changes we write any errors to Debug,
		//but for the time being our error checking code is extraneous.
		if(libpd_read_array(dest, src, offset, count) < 0)
			Debug.LogWarning(gameObject.name + "::ReadArray(): Array [" + src + "] does not exist OR the desired range lies outside the array's range.");
	}

	//--------------------------------------------------------------------------
	///	Writes an array of C# floats into the named Pd array.
	///	<param name="dest">Name of the array in your Pd patch.</param>
	///	<param name="offset">Offset into the Pd array to start writing
	///	 to.</param>
	///	<param name="src">The C# array of values to write into the Pd patch.
	///	 Make sure this contains >= count elements.</param>
	///	<param name="count">The number of elements to write into the Pd
	///	 array.</param>
	public void WriteArray(string dest, int offset, float[] src, int count)
	{
		libpd_set_instance(instance);

		//The same note (ReadArray) re: return values applies here.
		if(libpd_write_array(dest, offset, src, count) < 0)
			Debug.LogWarning(gameObject.name + "::WriteArray(): Array [" + dest + "] does not exist OR the desired range lies outside the array's range.");
	}
	#endregion

	#region delegate definitions
	//--------------------------------------------------------------------------
	/// Receive print messages.
	static void PrintOutput(string message)
	{
		if(pipePrintToConsoleStatic)
			Debug.Log("libpd: " + message);
	}

	//--------------------------------------------------------------------------
	/// Receive bang messages.
	void BangOutput(string symbol)
	{
		addAudioAction(() => {pureDataEvents.Bang.Invoke(symbol);});
	}

	//--------------------------------------------------------------------------
	/// Receive float messages.
	void FloatOutput(string symbol, float val)
	{
		addAudioAction(() => {pureDataEvents.Float.Invoke(symbol, val);});
	}

	//--------------------------------------------------------------------------
	/// Receive symbol messages.
	void SymbolOutput(string symbol, string val)
	{
		addAudioAction(() => {pureDataEvents.Symbol.Invoke(symbol, val);});
	}

	//--------------------------------------------------------------------------
	/// Receive lists.
	void ListOutput(string source, int argc, IntPtr argv)
	{
		var args = ConvertList(argc, argv);

		addAudioAction(() => {pureDataEvents.List.Invoke(source, args);});
	}

	//--------------------------------------------------------------------------
	/// Receive messages.
	void MessageOutput(string source, string symbol, int argc, IntPtr argv)
	{
		var args = ConvertList(argc, argv);

		addAudioAction(() => {pureDataEvents.Message.Invoke(source, symbol, args);});
	}

	//--------------------------------------------------------------------------
	///	Receive MIDI note on messages.
	void MidiNoteOnOutput(int channel, int pitch, int velocity)
	{
		addAudioAction(() => {midiEvents.MidiNoteOn.Invoke(channel, pitch, velocity);});
	}

	//--------------------------------------------------------------------------
	///	Receive MIDI control change messages.
	void MidiControlChangeOutput(int channel, int controller, int value)
	{
		addAudioAction(() => {midiEvents.MidiControlChange.Invoke(channel, controller, value);});
	}

	//--------------------------------------------------------------------------
	///	Receive MIDI program change messages.
	void MidiProgramChangeOutput(int channel, int program)
	{
		addAudioAction(() => {midiEvents.MidiProgramChange.Invoke(channel, program);});
	}

	//--------------------------------------------------------------------------
	///	Receive MIDI pitch bend messages.
	void MidiPitchBendOutput(int channel, int value)
	{
		addAudioAction(() => {midiEvents.MidiPitchBend.Invoke(channel, value);});
	}

	//--------------------------------------------------------------------------
	///	Receive MIDI aftertouch messages.
	void MidiAftertouchOutput(int channel, int value)
	{
		addAudioAction(() => {midiEvents.MidiAftertouch.Invoke(channel, value);});
	}

	//--------------------------------------------------------------------------
	///	Receive MIDI polyphonic aftertouch messages.
	void MidiPolyAftertouchOutput(int channel, int pitch, int value)
	{
		addAudioAction(() => {midiEvents.MidiPolyAftertouch.Invoke(channel, pitch, value);});
	}

	//--------------------------------------------------------------------------
	///	Receive MIDI byte messages.
	void MidiByteOutput(int channel, int value)
	{
		addAudioAction(() => {midiEvents.MidiByte.Invoke(channel, value);});
	}
	#endregion

	#region private methods
	//--------------------------------------------------------------------------
	///	Helper method used by SendList() and SendMessage().
	private void ProcessArgs(object[] args)
	{
		if(args.Length < 1)
			Debug.LogWarning(gameObject.name + "::ProcessArgs(): no arguments passed in for list or message.");
		else
		{
			if(libpd_start_message(args.Length) != 0)
				Debug.LogWarning(gameObject.name + "::ProcessArgs(): Could not allocate memory for list or message.");
			else
			{
				foreach(object arg in args)
				{
					if(arg is int?)
						libpd_add_float((float)((int?)arg));
					else if(arg is float?)
						libpd_add_float((float)((float?)arg));
					else if(arg is double?)
						libpd_add_float((float)((double?)arg));
					else if(arg is string)
						libpd_add_symbol((string)arg);
					else
						Debug.LogWarning(gameObject.name + "::ProcessArgs(): Cannot process argument of type " + arg.GetType() + " for list or message.");
				}
			}
		}
	}
	
	//--------------------------------------------------------------------------
	///	Helper method. Used by ListOutput() and MessageOutput().
	private object[] ConvertList(int argc, IntPtr argv)
	{
		var retval = new object[argc];

		for(int i=0;i<argc;++i)
		{
			if(libpd_is_float(argv) != 0)
				retval[i] = libpd_get_float(argv);
			else if(libpd_is_symbol(argv) != 0)
				retval[i] = Marshal.PtrToStringAnsi(libpd_get_symbol(argv));

			if(i < (argc-1))
				argv = libpd_next_atom(argv);
		}

		return retval;
	}
	
	//--------------------------------------------------------------------------
	/// Used to queue an event/action from the audio thread, to be dispatched in
	/// the main thread.
	private void addAudioAction(System.Action action) {
		if (action == null)
			throw new ArgumentNullException("action");

		lock(audioThreadActions) {
			audioThreadActions.Add(action);

			actionsPending = true;
		}
	}

	#endregion
}