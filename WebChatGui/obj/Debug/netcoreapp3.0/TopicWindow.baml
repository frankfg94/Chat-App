   M S B A M L   `   `   `  ����K  GWebChatGuiClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:WebChatGuiClientWebChatGuiClient  5   	   "    WebChatGuiClient.TopicWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:WebChatGuiClient     5   	     � 
ResizeMode$  NoResize=�5       � Title$ 
Quick Chat��60    � WindowStartupLocation$ CenterScreen=�5   
   $	��250��6/   $	��300��6<   .��5	      � -   5         �Name$
 grid��5	       � Cursor$
 Hand��6    � RowDefinitions 5
   
   �� 5      $��1* �6   �� 5      $��4* �6   �� 5      $	��10* �6   �� 5      $��1* �6    � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $	��10* �6   �� 5      $��1* �6   .+�5   
   ]� $��1��6   $��1��6$   .+�5      �� .+�5      �� 
�������$��16(�6.   $��
Topic Name��6=   �� 5      -   5         ��Name$ topicNameTbox��5      5      �� 5      $L��Fhttps://icon-library.net/images/group-chat-icon/group-chat-icon-19.jpg��6   5      �� 5   
   $��0 10 0 0q�6   $��2��6'   $��1��64   .+�5      �� 
�������$��16(�6*   $��Description��69   �� 5      -   5        $ topicDescriptionTbox��5      5      �� 5    
   -   5         ��Name$ optionsPanel��5       $��1��6,   $��2��6<   $��Bottom=�6I   .+�5!      �� -   5        	 ��Name$	 saveTopicButton��5!       $   MaterialDesignFlatAccentBgButton#��[  60   $��Create this topic��6j   5"      5#      5$      