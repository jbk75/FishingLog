                        function FormatDay(date)
                        {
                            var day = date.getDate();
                            var str = "" + day;
                            var pad = "00"
                            var dayFormatted = pad.substring(0, pad.length - str.length) + str;
                            return dayFormatted;
                        }

                        function FormatMonth(date)
                        {
                            var monthIndex = date.getMonth() + 1;
                            var str = "" + monthIndex;
                            var pad = "00"
                            var monthFormatted = pad.substring(0, pad.length - str.length) + str;
                            return monthFormatted;
                        }