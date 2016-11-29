using UnityEngine;
using UnityEditor;

namespace Parabox.Debug
{
	public static class ProfilerStyles
	{

		private static GUIStyle _borderStyle,
								_splitStyle,
								_chartStyle,
								_entryStyle,
								_chartAxisLabel,
								_chartBackgroundStyle;


		public static GUIStyle borderStyle
		{
			get
			{
				if(_borderStyle == null)
				{
					_borderStyle = new GUIStyle();
					_borderStyle.normal.background = Resources.Load<Texture2D>("Border");
					_borderStyle.border = new RectOffset(1,1,1,1);
				}
				return _borderStyle;
			}
		}

		public static GUIStyle splitStyle
		{
			get
			{
				if(_splitStyle == null)
				{
					_splitStyle = new GUIStyle();
					_splitStyle.normal.background = EditorGUIUtility.whiteTexture;
					_splitStyle.margin = new RectOffset(6,6,0,0);
				}
				return _splitStyle;
			}
		}

		public static GUIStyle chartStyle
		{
			get
			{
				if(_chartStyle == null)
				{
					_chartStyle = new GUIStyle(EditorStyles.toolbar);
					_chartStyle.normal.background = null;
					_chartStyle.onNormal.background = null;
				}
				return _chartStyle;
			}
		}

		public static GUIStyle entryStyle
		{
			get
			{
				if(_entryStyle == null)
				{
					_entryStyle = new GUIStyle(EditorStyles.toolbarButton);
					_entryStyle.normal.background = EditorGUIUtility.whiteTexture;
					_entryStyle.onNormal.background = EditorGUIUtility.whiteTexture;

					if(!EditorGUIUtility.isProSkin)
						entryStyle.normal.textColor = Color.black;
				}
				return _entryStyle;
			}
		}

		public static GUIStyle chartAxisLabel
		{
			get
			{
				if(_chartAxisLabel == null)
				{
					_chartAxisLabel = new GUIStyle(EditorStyles.whiteMiniLabel);
					_chartAxisLabel.alignment = TextAnchor.LowerLeft;
				}
				return _chartAxisLabel;
			}
		}

		public static GUIStyle chartBackgroundStyle
		{
			get
			{
				if(_chartBackgroundStyle == null)
				{
					_chartBackgroundStyle = new GUIStyle();
					_chartBackgroundStyle.border = new RectOffset(1,1,1,1);
					_chartBackgroundStyle.normal.background = Resources.Load<Texture2D>("Border");
				}
				return _chartBackgroundStyle;
			}
		}
	}
}
