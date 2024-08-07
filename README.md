# Unity-TimelineExtension
## There are two extensions here：
![cb2168f33921ff236b2a89b891a580ec](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/41eda3ba-3797-4684-9977-abb0fcc92943)

****
## Effect Track：
>You can drag in any prefab to create this track, supports particle/nested timeline playback. It also has a curve controller, you can use this to make a ballistic system.

![eaf93c8c-7eb6-4ef5-ac93-7762b6239711](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/5eecf9b9-37bc-4f41-8c74-ab0a733a5cdd)

instantiation parent:

![0cab34e9-beae-4aaa-a5ea-50dda9a9983e](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/db256fea-19fe-4ef6-97f7-b98faa28e014)


targets are refer this mono script:

![315fcace-1e4c-4226-baf2-b3209d1d1b2f](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/b15905a5-50fb-4672-ad4e-75f867bfb88f)

reference coordinate system:

![199d6ef3-2bf0-49f7-947e-560d220f68c4](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/fd78bcef-0561-4dd9-a8e1-7ed7253851bb)

select the generated node, default node is playable root gameobjcet:

![8a132d0a-75b9-40ee-bcf6-4e3d85664fc2](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/7f981401-7534-4753-a0bb-14219f1d93dd)

Rhythm function can be used to modify the time curve of the particle playback or nested timeline:

![45d4a45f-5825-47ac-be02-25b21d35d1db](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/cf7bc541-85a5-44c8-876b-2bf81c4de33d)

edit transform information: select this clip and switch the scene handle to rect tool

![9d82c955-14b8-476a-a5c1-abf9632f9c6b](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/b02b6be0-bcbf-4b4c-a833-69fed24cdd6a)

there is the curve controller, enable this button you can make a ballistic system. when clip selected, curves can be edit in scene

![c11faf68-8c68-4acc-85bc-374063a52bce](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/7d694608-f507-4c09-a6a7-60c2ef17d230)

![ab46990d-0abb-4020-8966-838d7adb741b](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/5e70a7c7-5b0e-473e-b2ee-f261685009af)


****
## Proxy Track：
>You can create a custom data to let the timeline control, supports float/vector/color/int

![image](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/4318aaba-7115-4444-9f41-4e329a015339)

specify a mono script to fetch custom data, need to inherit 'IPlayableControl' or 'ProxyControl'.  
( remember to mark the script with [ExecuteInEditMode]. and for details about how to get custom data, see 'ProxyTest.cs' )

![image](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/7d3cd572-c098-497b-83ab-35426b887430)

![e1dc4908-e854-453e-8e66-c94005daf205](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/f524fe5b-2e5d-46a5-9165-ae0d2cc34c1b)

![image](https://github.com/Kisawa/Unity-TimelineExtension/assets/71002504/f908f5b5-4a43-4339-876b-1e6cd8f40e42)



