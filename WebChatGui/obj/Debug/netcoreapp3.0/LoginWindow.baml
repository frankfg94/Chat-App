   M S B A M L   `   `   `  ����K  GWebChatGuiClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:WebChatGui
WebChatGui  5   	       WebChatGui.LoginWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:WebChatGui     5   	   S OMaterialDesignThemes.Wpf, Version=1.0.1.0, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   
������   � WindowStyle$
  None=�5   3    � Title$ 
Quick Chat��6F    � AllowsTransparency$
 True��5	   	   $	��450��6#   $	��800��60    � Cursor$
 Hand��6<   .��5      � -   5         � RowDefinitions 5   
   .+�5   
   �� $	��600��6   $	��400��6    �� CornerRadius$ 40��6,   $��20q�6>   ���	#FFF7EC52$��2��6a   ._�5      �� -   5         ��Name$ 
stackPanel��5      $	��400��6/   $��Center=�6;   $��Center=�6X   z�5      V� 5      .~�5      �� �� 5      �� 5      T� 5      5      .+�5      �� -   5         ��Name$ label��5      $��Center=�6'   $��Hello, please connect��6D   $��23(�6d   ���	#DD4E4A4A6r   �� 5      -   5        $ stackPanel1��5   =   $��Center=�6   $��0,20,0,0q�6R   $
��auto��6f    �� Orientation$ 
Horizontal=�6s   .+�5      '  !MaterialDesignThemes.Wpf.PackIcon  -   5        	  Name$	 packIcon��5   .   $��30��6A   $��30��6M   $��Bottom=�6Y   
   Kind$

 User=�6t   �� 5      -   5         ��Name$ 
usernameTB��5   \   
�������$��7,0,0,0q�63   $��1q�6D   $��30��6P   $	��300��6p    �� SelectionBrush
 � ���6|   5      �� 5      -   5        $ stackPanel2��5   :   $��Center=�6   $��Center=�6P   $��	0,20,0,20q�6l   $ 
Horizontal=�6   .+�5         -   5        $	 	packIcon1��5    .   $��30��6B   $��30��6N   $��Bottom=�6Y   $	
 Key=�6t   G� 5!      -	   5         G�Name$ userPassword��5!   ^   
�������$��7,0,0,0q�65   $��1q�6F   $��30��6R   $	��300��6t    G� SelectionBrush
 � ���6�   5"      �� 5#      -
   5         ��Name$ connectButton��5#      $��0 30 0 0q�60   
�������
��� ���$��14(�6i   $��40��6w   $��Login��6�   
��� ��� � 	Resources 5$       �� 
TargetType �� Property �� Value��%^   '��       �� 5%      " ��6    .�5&      ��  �@��CornerRadius$ 5��6=   5'      5(      5)      �� 5*      -   5         ��Name$ 
infoTBlock��5*   ?   $y�	Collapsed=�6   $
��Test��63   $
 Hand��6S   5-      5.      51      53      