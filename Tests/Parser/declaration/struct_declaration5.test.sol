<<<table #1>>>
<tags>
A record
   <<<table #2>>>
   <tags>
   B record
      <<<table #3>>>
      <tags>
      <types>
      <vars>
      var s type  record
                    <<<table #4>>>
                    <tags>
                    <types>
                    <vars>
                    var x type int
                    var y type int
                  endrecord
    endrecord
   <types>
   <vars>
   var b type B record
                 <<<table #3>>>
                 <tags>
                 <types>
                 <vars>
                 var s type  record
                               <<<table #4>>>
                               <tags>
                               <types>
                               <vars>
                               var x type int
                               var y type int
                             endrecord
               endrecord
   var b2 type B record
                  <<<table #3>>>
                  <tags>
                  <types>
                  <vars>
                  var s type  record
                                <<<table #4>>>
                                <tags>
                                <types>
                                <vars>
                                var x type int
                                var y type int
                              endrecord
                endrecord
 endrecord
<types>
int
char
double
void
<vars>
var printf type function printf(var  type  to char, ){
} returned void
var scanf type function scanf(var  type  to char, ){
} returned void
var getchar type function getchar(){
} returned int
var a type A record
              <<<table #2>>>
              <tags>
              B record
                 <<<table #3>>>
                 <tags>
                 <types>
                 <vars>
                 var s type  record
                               <<<table #4>>>
                               <tags>
                               <types>
                               <vars>
                               var x type int
                               var y type int
                             endrecord
               endrecord
              <types>
              <vars>
              var b type B record
                            <<<table #3>>>
                            <tags>
                            <types>
                            <vars>
                            var s type  record
                                          <<<table #4>>>
                                          <tags>
                                          <types>
                                          <vars>
                                          var x type int
                                          var y type int
                                        endrecord
                          endrecord
              var b2 type B record
                             <<<table #3>>>
                             <tags>
                             <types>
                             <vars>
                             var s type  record
                                           <<<table #4>>>
                                           <tags>
                                           <types>
                                           <vars>
                                           var x type int
                                           var y type int
                                         endrecord
                           endrecord
            endrecord

