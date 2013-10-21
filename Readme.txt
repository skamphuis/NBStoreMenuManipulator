40FINGERS DotNetNuke NbStoreMenuManipulator

Product of:
40Fingers 
Stefan Kamphuis 2013-10-21 

Version 01.00.00: Initial version
To install: install as any extension on DNN.
To use: 
Add a setting named "FortyFingers.CategoriesTab" to your website. Make sure you choose "Website Page" as the type. Although not required, you might want to add it to the System.Tabs categoryMake sure your skin uses the DDR Menu skinobject for rendering the menuAdd the following attribute to the menu control in the skin:
NodeManipulator="FortyFingers.NbStoreMenuManipulator.NbStoreMenuManipulator,40Fingers.DNN.Modules.NbStoreMenuManipulator"This component will replace the tab selected in the setting (which is localizable) by the categories in the store. This way you will only need one productlist page, but can still have all categories in your menu. 
Also, you can choose to have specific categries displayed on other tabs. To do this: 
Create a setting named: manipulatorcatidontabid.textSet the value for the new seeting with semicolon separated references for each categroy that needs to be displayed on another tab:
first-categoryId>new-tabId;second-categoryId>other-tabId
for example: 3>155;4>162 The component will make no changes to the new tab. DNN Controls it's name and wether it's in the menu or not. 