   M S B A M L   `   `   `  ����H  DHitomi Viewer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:Hitomi_ViewerHitomi_Viewer  5   	       Hitomi_Viewer.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:Hitomi_Viewer     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      �  � RowDefinitions 5
   
   �� 5      $	��64* �6   �� 5      $
��355* �6   .+�5   
   �� $��0��6   $��Stretch=�6"   $��Stretch=�6@   $��5 5 5 5q�6\   .+�5      �� -   5         ��Name$ search��5      $��검색��6%   $��Center=�62   $��Center=�6O   $��77��6j   $��690 0 0 -50q�6u   $��22��6�   6�   �� 5      -   5         ��Name$ keyword��5      $�� ��6'   $��Center=�6/   $��Center=�6L   $	��675��6g   $��22��6s   $��-80 0 0 -50q�6   .��6�   ��5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5   1   �� 6   6sys&clr-namespace:System;assembly=mscorlib     61    �� 
TargetType" ��6    �� 	Resources 5         CueBannerBrush j� 
AlignmentX j� 
AlignmentY j� Stretch	 !� Visual��%�   &	        !� 5      $
 Left=�6A   $ Center=�6S   $
 None=�6g   	 5   "   �� 5   &   $ ��여기에 번호 입력...��6,   ���	#FFA2A2A26C   5      5      
 �� Triggers
 5      P� 5       P� Property � �� �� Empty P� Value# Z 66   . �5   "   ��  �� Property � �� �� Value# [  6?   5      P� 5       � �� 66   Z� . �5   "   ��  � ��# [  6?   5       P� 5!       �@J�IsKeyboardFocused . . �5"   "   ��  � ��
 �����6?   5#      5%      .��5&   !   5'      5(      � 5)   
   $��Stretch=�6   $��5 5 5 5q�6-   $��1��6>   $��Stretch=�6K    � ColumnDefinitions 5*      �� 5+      $
��128* �6#   �� 5,      $
��263* �6#   .+�5.      �� -   5         ��Name$ 	thumbnail��5.      $��5 5 5 5q�6'   $��UniformToFill=�68   5/      51      52      