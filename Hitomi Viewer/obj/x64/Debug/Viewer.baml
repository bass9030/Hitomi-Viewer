   M S B A M L   `   `   `  ����  Hitomi Viewer    Hitomi_Viewer.Viewer   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      U QWpfAnimatedGif, Version=2.0.0.0, Culture=neutral, PublicKeyToken=9e7cd3b544a090dc,gif"http://wpfanimatedgif.codeplex.com  5        N� Title$  Viewer��5      $	��831��6   .��5	      �  � RowDefinitions 5
   
   �� 5      $	��13* �6   �� 5      $	��30* �6   �� 5      $	��10* �6   �� 5      $
��500* �6   .+�5   
   ]� $��0��6   .+�5      o� $	��Top=�6   .@�5      m� $��보기��6   .@�5      m� -   5         m�Name$ is_full_spread��5   1   $��풀페이지로 보기��6    m� IsCheckable .  m� 	IsChecked . 6�   m� 5      -   5        $ goto_search��5      $"��검색창으로 돌아가기��64   6H   5      5      5      �� 5   
   -   5         ��Name$ page_num��5      $��1��6%   $	��100��62   $��10 0 0 0q�6>   $
��Left=�6P   $��Center=�6k   6�   � 5   
   -   5         �Name$ loading_bar��5   #   $��2��6   6#   � 5   
   -   5         �Name$ Single��5      $��3��6   $y�Hidden=�6,    � ColumnDefinitions 5      �� 5      $
��100* �6#   �� 5      $
��180* �6#   �� 5      $
��100* �6#   .+�5       � -   5        $ single_page_grid��5       $��0 0 0 0q�6-   $��Center=�6>   $��Center=�6Y   $��1��6v   
������ .+�5!      �� -   5        	 ��Name$	 single_page��5!      "  WpfAnimatedGif.ImageBehavior
   AnimatedSource$
 Loading.gif��6-   $
��None=�6\   5"      5#      � 5$   
   -   5        $
 full��5$      $��3��6   $y�Visible=�6*    5%      �� 5&      $
��500* �6#   �� 5'      $
��500* �6#   .+�5)      � -	   5        $ full_page_1_grid��5)      $��0��6-   
������ .+�5*      �� -
   5        $	 full_page_1��5*      $
 Loading.gif��6-   $
��None=�6\   5+      � 5,      -   5        $ full_page_2_grid��5,      $��1��6-   
������ .+�5-      �� -   5        $	 full_page_2��5-      $
 Loading.gif��6-   $
��None=�6\   5.      5/      50      51      