mergeInto(LibraryManager.library, {
  UploadFile: function (gameObjectName, methodName, filter, multiple) {
    var gameObject = UTF8ToString(gameObjectName);
    var method = UTF8ToString(methodName);
    var filterString = UTF8ToString(filter);
    
    var input = document.createElement('input');
    input.type = 'file';
    input.accept = filterString;
    input.multiple = multiple;
    
    input.onchange = function (e) {
      var files = e.target.files;
      var file = files[0];
      var reader = new FileReader();
      
      reader.onload = function(event) {
        var contents = event.target.result;
        unityInstance.SendMessage(gameObject, method, contents);
      };
      
      reader.readAsText(file);
    };
    
    input.click();
  }
}); 