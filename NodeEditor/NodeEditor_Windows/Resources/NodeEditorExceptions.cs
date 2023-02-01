using System;
using System.Collections.Generic;
using BixBite.NodeEditor;

// ReSharper disable IdentifierTypo

// <summary>
// this is a class that i use to hold all the custom errors for the node block editor.
// </summary>
namespace BixBite.Resources
{

	public class NodeEditorException : Exception
	{
		public NodeEditorException(String Message) : base(Message)
		{

		}
	}

	/// <summary>
	/// This is here to be thrown when an Input node doesn't have a valid connection.
	/// </summary>
	public class InputNodeConnectionException : NodeEditorException
	{

	  public InputNodeConnectionException(int nodenum, string blocktype)
		  : base($"Input Node: {nodenum} on {blocktype} is missing a connection!")
	  {

	  }

	}

	/// <summary>
	/// This is here to be thrown when an Output node doesn't have a valid connection.
	/// </summary>
	public class OutputNodeConnectionException : NodeEditorException
	{
		public OutputNodeConnectionException(int nodenum, string blocktype)
			: base($"Output Node: {nodenum} on {blocktype} is missing a connection!")
		{

		}
	}

	/// <summary>
	/// This is here to be thrown when an Entry node doesn't have a valid connection.
	/// </summary>
	public class EntryNodeConnectionException : NodeEditorException
	{
		public EntryNodeConnectionException(string blocktype)
			: base($"Entry Node on {blocktype} is missing a connection!")
		{

		}
	}

	/// <summary>
	/// This is here to be thrown when an exit node doesn't have a valid connection.
	/// </summary>
	public class ExitNodeConnectionException : NodeEditorException
	{
		public ExitNodeConnectionException(string blocktype)
			: base($"Exit Node on {blocktype} is missing a connection!")
		{

		}
	}

	/// <summary>
	/// This is here to be thrown when a -1 or > dialogue.outputs.length is inputted
	/// into the choice var (1st input)
	/// </summary>
	public class DialogueChoiceInvalidException : NodeEditorException
	{
		public DialogueChoiceInvalidException(int choicevar, List<ConnectionNode> connecList)
			: base($"Dialogue Block ChoiceVar is an Invalid value. Found [{choicevar}] : " +
			       $"Expected 0 - {connecList.Count-1}" )
		{

		}
	}
	public class ChangeVarFlag : NodeEditorException
	{
		public RuntimeVars VarToChange;
		public RuntimeVars NewVar;
		public ChangeVarFlag(RuntimeVars rtv, RuntimeVars newvar)
			: base($"Updated Runtime Var. Var Name[{newvar.VarName}] => Was [{rtv.VarData}] : Now is [{newvar.VarData}]")
		{
			VarToChange = rtv;
			NewVar = newvar;
		}
	}

}
