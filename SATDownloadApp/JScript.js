function getValue(name) {
    var type='',value='',n=0;
  
    // get the type of variable 
    try { 
        type=eval('typeof '+name); 
    } catch (e) { 
        type=e.message; 
    }

    try {
        if (eval('typeof '+name)!='object') 
            // get the value of simple variable
            value=eval(name);
        else {
            // for-in loop to get the names and values of children of a object
            for (var child in eval(name)) {
                try {
                    value+='.'+child+'='+eval(name+'.'+chid)+'; ';
                } catch(e) {
                    value+='.'+child+'=error:'+e.message+'; ';
                }
                
                // only return the first 20 children if it is a object variable
                if (n++>=20) break;
            }
        }
    } catch (ex) { 
        value='Error: '+ex.message; 
    }
    
    // return '<td>[simple vaiable name]</td><td>[value]</td>[type]</td>' or
    // return '<td>[object name]</td><td>.[child name][value]</td>object</td>'
    return '<td>'+name+'</td><td>'+( (n<20)?'':'<font color=\"gray\">First 20 properties:</font><br />' )+value+'</td><td>'+type+'</td>';
}

window.external.getScriptResult({0});      
